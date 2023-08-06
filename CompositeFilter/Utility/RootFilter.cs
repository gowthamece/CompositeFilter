namespace CompositeFilter.Utility
{

         public class RootFilter
        {
            public List<Filter> Filters { get; set; }
            public string Logic { get; set; }
        }

        public class Filter
        {
            public string Field { get; set; }
            public string Operator { get; set; }
            public object Value { get; set; }
            public string Logic { get; set; } // This is the nested "logic" property for the inner filters array
            public List<Filter> Filters { get; set; } // Nested filters array
        }
    
}
