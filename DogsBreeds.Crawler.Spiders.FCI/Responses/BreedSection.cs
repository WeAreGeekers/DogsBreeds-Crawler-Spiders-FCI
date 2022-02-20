namespace WeAreGeekers.DogsBreeds.Crawler.Spiders.FCI.Responses
{

    /// <summary>
    /// Response object of Breed Section
    /// </summary>
    public class BreedSection
    {

        /// <summary>
        /// Group ref to this section
        /// </summary>
        public BreedGroup Group { get; set; }

        /// <summary>
        /// Index of section
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Official name of section
        /// </summary>
        public string OfficialName { get; set; }

        /// <summary>
        /// List of sub section of this breed section
        /// </summary>
        public List<BreedSubSection> SubSections { get; set; }

    }

}
