using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {

        #region Properties
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        #endregion

        #region Constructor
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }
        #endregion

        #region Actions

        [HttpGet]
        public IActionResult Index()
        {
            //get user id
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //get all products that are associated with current ApplicationUserId
            ShoppingCartVM shoppingCartVM = new()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(e => e.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            //set prices for each product in the cart
            foreach (var obj in shoppingCartVM.ListCart)
            {
                obj.Price = GetPriceBasedOnQuantity(obj.Count, obj.Product.Price, obj.Product.Price50, obj.Product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += (1.0 * obj.Count * obj.Price);
            }

            return View(shoppingCartVM);
        }

        [HttpGet]
        public IActionResult Summary()
        {
            //find the ApplicationUserId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //now create cart object
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(e => e.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            //get the user inside ApplicationUser
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claims.Value);

            //Assign Properties
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;

            //Update Cart price and calculate order total
            foreach (var obj in ShoppingCartVM.ListCart)
            {
                obj.Price = GetPriceBasedOnQuantity(obj.Count, obj.Product.Price, obj.Product.Price50, obj.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (1.0 * obj.Count * obj.Price);
            }

            //return to the view to show the info
            return View(ShoppingCartVM);
        }

        [ActionName("Summary")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST()
        {
            //find the ApplicationUserId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //get cart of the user
            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(e => e.ApplicationUserId == claims.Value, includeProperties: "Product");

            //define properties
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;


            //Update Cart price and calculate order total
            foreach (var obj in ShoppingCartVM.ListCart)
            {
                obj.Price = GetPriceBasedOnQuantity(obj.Count, obj.Product.Price, obj.Product.Price50, obj.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (1.0 * obj.Count * obj.Price);
            }

            //if company user then payment -> delayed && orderstatus -> approved
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claims.Value);

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            //create order header
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            //create order details for each items
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {


                //stripe settings
                var domain = "https://localhost:7093/";
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"customer/cart/index",
                };

                foreach (var item in ShoppingCartVM.ListCart)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            },
                        },
                        Quantity = item.Count,
                    };

                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);

                //ShoppingCartVM.OrderHeader.SessionId = session.Id;
                //ShoppingCartVM.OrderHeader.PaymentIntentId = session.PaymentIntentId;

                _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

                //stripe end
            }
            else
            {
                return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
            }


        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id, includeProperties:"ApplicationUser");

            //if not company user then we need to take payment
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //check stripe status
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, orderHeader.SessionId, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            //send email to the user
            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book", "Order Placed Successfully");

            //we need to remove this products from cart after order is placed
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            //clear session so cart count will be updated
            HttpContext.Session.Clear();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            //return to the view to show the info
            return View(id);

        }

        /// <summary>
        /// Add one product to card
        /// </summary>
        /// <param name="cartId">ID of the cartObj</param>
        /// <returns></returns>
        public IActionResult Plus(int cartId)
        {
            ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);

            _unitOfWork.ShoppingCart.IncrementCount(shoppingCart, 1);
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Remove one product from the cart
        /// </summary>
        /// <param name="cartId">ID of the cartObj</param>
        /// <returns></returns>
        public IActionResult Minus(int cartId)
        {
            ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);

            if (shoppingCart.Count == 1)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingCart);

                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetInt32(SD.SessionCart, count-1);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(shoppingCart, 1);
            }
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Remove selected product from the cart
        /// </summary>
        /// <param name="cartId">ID of the cartObj</param>
        /// <returns></returns>
		public IActionResult Remove(int cartId)
        {
            ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);

            _unitOfWork.ShoppingCart.Remove(shoppingCart);
            _unitOfWork.Save();

            var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32(SD.SessionCart, count);

            return RedirectToAction("Index");
        }
        #endregion

        #region Functions
        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity > 100)
            {
                return price100;
            }
            else if (quantity > 50)
            {
                return price50;
            }
            return price;
        }
        #endregion
    }
}
