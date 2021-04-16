using System;

namespace Searchlight
{
    /// <summary>
    /// Represents a field that is permitted to be used as a filter or sort-by column in the SafeParser
    /// </summary>
    public class SearchlightModel : Attribute
    {
        /// <summary>
        /// Information about the name of the field and its type in the database, if that type is different from the type the user sees.
        /// You can specify the type the user sees as well as the type as it is stored in the database.
        /// </summary>
        /// <param name="originalName">If the model is known by a different name in the underlying data store, specify it here</param>
        /// <param name="aliases">If the model will be known by multiple names, specify them here</param>
        /// <param name="maxParams">The maximum number of parameters that can be used in a query on this data source</param>
        public SearchlightModel(string originalName = null, string[] aliases = null, string maxParams = null)
        {
            OriginalName = originalName;
            Aliases = aliases ?? new string[] {};
            if (!String.IsNullOrWhiteSpace(maxParams))
            {
                MaximumParameters = Int32.Parse(maxParams);
            }
        }

        /// <summary>
        /// The underlying name for this model in the data store
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        /// If Searchlight should recognize this table by any other aliases, list them here
        /// </summary>
        public string[] Aliases { get; set; }
        
        /// <summary>
        /// The maximum number of parameters that can be used when querying this model
        /// </summary>
        public int? MaximumParameters { get; set; }
    }
}
