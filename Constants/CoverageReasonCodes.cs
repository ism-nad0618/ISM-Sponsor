using ISMSponsor.Models.API;

namespace ISMSponsor.Constants
{
    public static class CoverageReasonCodes
    {
        // Success codes
        public const string FULL_COVERAGE_ITEM = "FULL_COVERAGE_ITEM";
        public const string FULL_COVERAGE_CATEGORY = "FULL_COVERAGE_CATEGORY";
        public const string PERCENTAGE_COVERAGE_ITEM = "PERCENTAGE_COVERAGE_ITEM";
        public const string PERCENTAGE_COVERAGE_CATEGORY = "PERCENTAGE_COVERAGE_CATEGORY";
        public const string FIXED_AMOUNT_COVERAGE_ITEM = "FIXED_AMOUNT_COVERAGE_ITEM";
        public const string FIXED_AMOUNT_COVERAGE_CATEGORY = "FIXED_AMOUNT_COVERAGE_CATEGORY";

        // Partial coverage codes
        public const string CAP_REACHED_ITEM = "CAP_REACHED_ITEM";
        public const string CAP_REACHED_CATEGORY = "CAP_REACHED_CATEGORY";
        public const string PERCENTAGE_SPLIT = "PERCENTAGE_SPLIT";
        public const string FIXED_SPLIT = "FIXED_SPLIT";

        // Not covered codes
        public const string NO_ACTIVE_LOG = "NO_ACTIVE_LOG";
        public const string NO_MATCHING_RULE = "NO_MATCHING_RULE";
        public const string RULE_EXPIRED = "RULE_EXPIRED";
        public const string RULE_NOT_YET_EFFECTIVE = "RULE_NOT_YET_EFFECTIVE";
        public const string RULE_INACTIVE = "RULE_INACTIVE";
        public const string LOG_INACTIVE = "LOG_INACTIVE";

        // Validation failure codes
        public const string INVALID_STUDENT = "INVALID_STUDENT";
        public const string INVALID_ITEM = "INVALID_ITEM";
        public const string INVALID_AMOUNT = "INVALID_AMOUNT";
        public const string MISSING_ITEM_OR_CATEGORY = "MISSING_ITEM_OR_CATEGORY";
        public const string SPONSOR_LOG_MISMATCH = "SPONSOR_LOG_MISMATCH";

        public static List<ReasonCodeInfo> GetAllReasonCodes()
        {
            return new List<ReasonCodeInfo>
            {
                new ReasonCodeInfo { Code = FULL_COVERAGE_ITEM, Description = "Full coverage provided by item-level rule", Category = "Success" },
                new ReasonCodeInfo { Code = FULL_COVERAGE_CATEGORY, Description = "Full coverage provided by category-level rule", Category = "Success" },
                new ReasonCodeInfo { Code = PERCENTAGE_COVERAGE_ITEM, Description = "Percentage coverage applied by item-level rule", Category = "Success" },
                new ReasonCodeInfo { Code = PERCENTAGE_COVERAGE_CATEGORY, Description = "Percentage coverage applied by category-level rule", Category = "Success" },
                new ReasonCodeInfo { Code = FIXED_AMOUNT_COVERAGE_ITEM, Description = "Fixed amount coverage applied by item-level rule", Category = "Success" },
                new ReasonCodeInfo { Code = FIXED_AMOUNT_COVERAGE_CATEGORY, Description = "Fixed amount coverage applied by category-level rule", Category = "Success" },
                
                new ReasonCodeInfo { Code = CAP_REACHED_ITEM, Description = "Coverage capped by item-level rule limit", Category = "Partial" },
                new ReasonCodeInfo { Code = CAP_REACHED_CATEGORY, Description = "Coverage capped by category-level rule limit", Category = "Partial" },
                new ReasonCodeInfo { Code = PERCENTAGE_SPLIT, Description = "Partial coverage based on percentage rule", Category = "Partial" },
                new ReasonCodeInfo { Code = FIXED_SPLIT, Description = "Partial coverage based on fixed amount rule", Category = "Partial" },
                
                new ReasonCodeInfo { Code = NO_ACTIVE_LOG, Description = "No active Letter of Guarantee found for student", Category = "Failure" },
                new ReasonCodeInfo { Code = NO_MATCHING_RULE, Description = "No coverage rule matches the item or category", Category = "Failure" },
                new ReasonCodeInfo { Code = RULE_EXPIRED, Description = "Matching rule has expired", Category = "Failure" },
                new ReasonCodeInfo { Code = RULE_NOT_YET_EFFECTIVE, Description = "Matching rule is not yet effective", Category = "Failure" },
                new ReasonCodeInfo { Code = RULE_INACTIVE, Description = "Matching rule is inactive", Category = "Failure" },
                new ReasonCodeInfo { Code = LOG_INACTIVE, Description = "Letter of Guarantee is inactive", Category = "Failure" },
                
                new ReasonCodeInfo { Code = INVALID_STUDENT, Description = "Student not found or invalid", Category = "Validation" },
                new ReasonCodeInfo { Code = INVALID_ITEM, Description = "Item or category not found or invalid", Category = "Validation" },
                new ReasonCodeInfo { Code = INVALID_AMOUNT, Description = "Amount must be greater than zero", Category = "Validation" },
                new ReasonCodeInfo { Code = MISSING_ITEM_OR_CATEGORY, Description = "Either ItemId or CategoryId must be provided", Category = "Validation" },
                new ReasonCodeInfo { Code = SPONSOR_LOG_MISMATCH, Description = "Provided SponsorId does not match LoG sponsor", Category = "Validation" }
            };
        }
    }
}
