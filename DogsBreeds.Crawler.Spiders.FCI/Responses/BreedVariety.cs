namespace WeAreGeekers.DogsBreeds.Crawler.Spiders.FCI.Responses
{

    /// <summary>
    /// Response object of Breed Variety
    /// </summary>
    public class BreedVariety
    {
        
        /// <summary>
        /// Index letter of variety
        /// </summary>
        public string IndexLetter { get; set; }

        /// <summary>
        /// Official name of variety
        /// </summary>
        public string OfficialName { get; set; }

        /// <summary>
        /// Variety CACIB
        /// </summary>
        public bool Cacib { get; set; }

        /// <summary>
        /// List of sub varieties
        /// </summary>
        public List<BreedSubVariety> SubVarieties { get; set; }

    }

}
