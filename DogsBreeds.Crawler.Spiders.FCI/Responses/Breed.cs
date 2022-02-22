using WeAreGeekers.DogsBreeds.Crawler.Spiders.FCI.Enums;

namespace WeAreGeekers.DogsBreeds.Crawler.Spiders.FCI.Responses
{

    /// <summary>
    /// Response object of Breed
    /// </summary>
    public class Breed
    {

        /// <summary>
        /// Ref to breed group
        /// </summary>
        public BreedGroup Group { get; set; }

        /// <summary>
        /// Ref to breed section
        /// </summary>
        public BreedSection Section { get; set; }

        /// <summary>
        /// Ref to breed sub-section
        /// </summary>
        public BreedSubSection SubSection { get; set; }

        /// <summary>
        /// Idenitfy code of breed
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Official name of breed
        /// </summary>
        public string OfficialName { get; set; }

        /// <summary>
        /// Iso lang of official name of breed
        /// </summary>
        public string IsoOfficialLang { get; set; }

        /// <summary>
        /// The fci website page of the breed
        /// </summary>
        public string DetailPage { get; set; }

        /// <summary>
        /// List with official translation of the name of the breed (Tuple: iso lang; translation)
        /// </summary>
        public List<Tuple<string, string>> OfficialNameTranslations { get; set; }

        /// <summary>
        /// List of official pubblication in different languages of the breed (Tuple: iso lang; date pubblication; uri of pubblication)
        /// </summary>
        public List<Tuple<string, DateTime, string>> Pubblications { get; set; }

        /// <summary>
        /// Date of acceptance by fci in provisional way of the breed
        /// </summary>
        public DateTime? DateOfAcceptanceOnProvisionalBasisByTheFci { get; set; }

        /// <summary>
        /// Date of pubblication of official standard by fci
        /// </summary>
        public DateTime? DateOfPubblicationOfTheOfficialValidStandard { get; set; }

        /// <summary>
        /// The status of the breed
        /// </summary>
        public BreedStatus Status { get; set; }

        /// <summary>
        /// Origin countries of the breed
        /// </summary>
        public string[] OriginCountries { get; set; }

        /// <summary>
        /// Working trial status of the breed
        /// </summary>
        public BreedWorkingTrial WorkingTrial { get; set; }

        /// <summary>
        /// Patronage countries of the breed
        /// </summary>
        public string[] PatronageCountries { get; set; }

        /// <summary>
        /// Date of acceptance by fci in definitive way of the breed
        /// </summary>
        public DateTime? DateOfAcceptanceOnDefinitiveBasisByTheFci { get; set; }

        /// <summary>
        /// Devlopment countries of the breed
        /// </summary>
        public string[] DevelopmentCountries { get; set; }

        /// <summary>
        /// Define if the section is 'cacib' or not (Can be false and true in varieties of subsection)
        /// </summary>
        public bool Cacib { get; set; }

        /// <summary>
        /// List of varieties of the breed
        /// </summary>
        public List<BreedVariety> Varieties { get; set; }

        /// <summary>
        /// List of images of the breed
        /// </summary>
        public List<string> ListImages { get; set; }

        /// <summary>
        /// List of uri with resources of educations
        /// </summary>
        public List<Tuple<string, string, string>> EducationResources { get; set; }

    }

}
