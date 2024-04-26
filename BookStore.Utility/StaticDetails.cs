using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Utility
{
    public static class StaticDetails
    {
        // "c"onst value must be known from the "c"ompile time
        // "r"eadonly value either known from the "r"untime or assigned in the constructor
        public const string Customer_Role = "Customer";
        public const string Company_Role = "Company";
        public const string Admin_Role = "Admin";
        public const string Employee_Role = "Employee";
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymentStatusRejected = "Rejected";

        public const string SessionCart = "SessionShoppingCart";
    }
}
