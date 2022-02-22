using HtmlAgilityPack;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using WeAreGeekers.DogsBreeds.Crawler.Spiders.FCI.Enums;
using WeAreGeekers.DogsBreeds.Crawler.Spiders.FCI.Responses;

namespace WeAreGeekers.DogsBreeds.Crawler.Spiders.FCI
{

    /// <summary>
    /// Object with methods to extract by web-scraping the data from FCI (www.fci.be)
    /// </summary>
    public class FCISpider    
    {

        #region Private Const

        /// <summary>
        /// Fci site base uri
        /// </summary>
        private static readonly string FCI_BASE_URI = "http://fci.be";

        /// <summary>
        /// Fci website page uri list of provisional breeds page
        /// </summary>
        private static readonly string FCI_URI_PROVISIONAL_PAGE = "http://fci.be/en/nomenclature/provisoire.aspx";

        /// <summary>
        /// Fci website page uri list of breed groups page
        /// </summary>
        private static readonly string FCI_URI_BREED_GROUPS = "http://fci.be/en/Nomenclature/";

        #endregion


        #region Private Properties

        /// <summary>
        /// Define if we need to cache data or not
        /// </summary>
        private bool _cacheData { get; set; }

        /// <summary>
        /// List of detail pages of definitive breeds
        /// </summary>
        private List<string> _listDetailPagesDefinitivelBreeds { get; set; }

        /// <summary>
        /// List of detail pages of provisional breeds
        /// </summary>
        private List<string> _listDetailPagesProvisionalBreeds { get; set; }

        /// <summary>
        /// List of cached breed groups
        /// </summary>
        private List<BreedGroup> _listBreedGroups { get; set; }

        /// <summary>
        /// List of cached breed sections
        /// </summary>
        private List<BreedSection> _listBreedSections { get; set; }

        #endregion


        #region Constructor

        /// <summary>
        /// Object with methods to extract by web-scraping the data from FCI (www.fci.be)
        /// </summary>
        public FCISpider(bool cacheData)
        {
            _cacheData = cacheData;
            ClearCache();
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Method that clear the data caching
        /// </summary>
        public void ClearCache()
        {
            _listDetailPagesDefinitivelBreeds = null;
            _listDetailPagesProvisionalBreeds = null;
            _listBreedGroups = null;
            _listBreedSections = null;
        }

        /// <summary>
        /// Get the list of all breeds groups recognize from FCI
        /// </summary>
        /// <returns></returns>
        public List<BreedGroup> GetBreedGroups()
        {
            // Check cache data
            if (_cacheData && _listBreedGroups != null) return _listBreedGroups;

            // Init var
            List<BreedGroup> listBreedGroups = new List<BreedGroup>();

            // Extract data
            listBreedGroups = ExtractListOfBreedGroups();

            // Cache data
            if (_cacheData)
            {
                _listBreedGroups = listBreedGroups;
            }

            // Return breed groups
            return listBreedGroups;
        }

        /// <summary>
        /// Get the list of all breed sections recognize from FCI
        /// </summary>
        /// <returns></returns>
        public List<BreedSection> GetBreedSections()
        {
            // Check cache data
            if (_cacheData && _listBreedSections != null) return _listBreedSections;

            // Init var
            List<BreedSection> listBreedSections = new List<BreedSection>();

            // Extract groups and iterate it getting sections
            List<BreedGroup> listBreedGroups = GetBreedGroups();

            listBreedGroups.ForEach(breedGroup =>
            {
                listBreedSections.AddRange(ExtractListOfBreedSectionOfBreedGroup(breedGroup));
            });

            // Cache data
            if (_cacheData)
            {
                _listBreedSections = listBreedSections;
            }

            // Return breed sections
            return listBreedSections;
        }

        /// <summary>
        /// Get the list of all breeds recognize from FCI (definitive and provisional)
        /// </summary>
        /// <returns></returns>
        public List<Breed> GetBreeds()
        {
            // Init var
            List<Breed> listBreeds = new List<Breed>();

            // Add definitive breeds
            listBreeds.AddRange(GetDefinitiveBreeds());

            // Add provisional breeds
            listBreeds.AddRange(GetProvisionalBreeds());

            // Return breeds
            return listBreeds;
        }

        /// <summary>
        /// Get the list of all definitive breeds recognize from FCI
        /// </summary>
        /// <returns></returns>
        public List<Breed> GetDefinitiveBreeds()
        {
            // Init var
            List<Breed> listBreeds = new List<Breed>();

            // Extract detail pages of definitive breeds
            List<string> listDetailPagesDefinitiveBreeds = ExtractListOfDetailsPagesOfDefinitiveBreed();

            // Iterate detail pages
            listDetailPagesDefinitiveBreeds.ForEach(detailPage =>
            {
                listBreeds.Add(ExtractDataFromBreedDetailPage(detailPage));
            });

            // Extract info about 'CACIB' of breed 
            listBreeds = GetBreedCabibData(listBreeds);

            // Return breeds
            return listBreeds;
        }

        /// <summary>
        /// Get the list of all provisional breeds recognize from FCI
        /// </summary>
        /// <returns></returns>
        public List<Breed> GetProvisionalBreeds()
        {
            // Init var
            List<Breed> listBreeds = new List<Breed>();

            // Extract detail pages of provisional breeds
            List<string> listDetailPagesProvisionalBreeds = ExtractListOfDetailsPagesOfProvisionalBreed();

            // Iterate detail pages
            listDetailPagesProvisionalBreeds.ForEach(detailPage =>
            {
                listBreeds.Add(ExtractDataFromBreedDetailPage(detailPage));
            });

            // Return breeds
            return listBreeds;
        }

        #endregion


        #region Private Methods (Utilities)

        /// <summary>
        /// Private method that transform the label lang into iso code 
        /// </summary>
        /// <param name="labelLang"></param>
        /// <returns></returns>
       
        private string GetIsoCodeLang(string labelLang)
        {
            switch (labelLang.ToLower())
            {
                case "english": 
                    return "en";

                case "french":
                case "français":
                    return "fr";

                case "german":
                case "deutsch":
                    return "de";

                case "spanish":
                case "español":
                    return "es";

                default:
                    throw new NotImplementedException(
                        string.Format(
                            "Warning! labelLang param equals to '{0}' not supported yet.",
                            labelLang.ToLower()
                        )
                    );
            }
        }

        #endregion


        #region Private Methods (Extract)

        /// <summary>
        /// Private method that extract the list of breed groups
        /// </summary>
        /// <returns></returns>
        private List<BreedGroup> ExtractListOfBreedGroups()
        {
            // Init var
            List<BreedGroup> listBreedGroups = new List<BreedGroup>();

            // Download html page and parse it
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(FCI_URI_BREED_GROUPS);

            // Get list of data
            listBreedGroups = doc.DocumentNode
                .SelectNodes("//div[@class='group']")
                .Select(s => new BreedGroup()
                {
                    Index = int.Parse(s.Descendants("a").FirstOrDefault().InnerHtml.ToUpper().Replace("GROUP", string.Empty).Trim()),
                    OfficialName = s.ParentNode.Descendants("span").FirstOrDefault().InnerHtml.Trim(),
                    DetailPage = FCI_BASE_URI + s.Descendants("a").FirstOrDefault().GetAttributeValue<string>("href", string.Empty)
                })
                .ToList();

            // return data
            return listBreedGroups;
        }

        /// <summary>
        /// Private method that extract the list of breed sections of breed groups
        /// </summary>
        /// <param name="breedGroup"></param>
        /// <returns></returns>
        private List<BreedSection> ExtractListOfBreedSectionOfBreedGroup(BreedGroup breedGroup)
        {
            // Init var
            List<BreedSection> listBreedSections = new List<BreedSection>();

            // Download html page and parse it
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(breedGroup.DetailPage);

            // Extract data
            listBreedSections = doc.DocumentNode.SelectNodes("//ul[@class='sections']").Elements("li")
                .Select(ulSection => 
                {
                    BreedSection breedSections = new BreedSection()
                    {
                        Group = breedGroup,
                        Index = int.Parse(ulSection.Descendants("span").FirstOrDefault().Descendants("b").FirstOrDefault().InnerText.ToUpper().Replace("SECTION", string.Empty).Replace(":", string.Empty).Trim()),
                        OfficialName = ulSection.Descendants("span").FirstOrDefault().InnerText.Trim()
                            .Substring(ulSection.Descendants("span").FirstOrDefault().InnerText.Trim().IndexOf(":") + 1)
                            .Trim(),
                        SubSections = ulSection.Descendants("ul").Where(w => w.HasClass("soussections")).SelectMany(s => s.Elements("li"))
                            .Select(s =>
                            {
                                BreedSubSection breedSubSection = new BreedSubSection()
                                {
                                    Index = int.Parse(
                                        s.Descendants("span").FirstOrDefault().InnerText.Trim()
                                        .Substring(0, s.Descendants("span").FirstOrDefault().InnerText.Trim().IndexOf(' '))
                                        .Replace(ulSection.Descendants("span").FirstOrDefault().Descendants("b").FirstOrDefault().InnerText.ToUpper().Replace("SECTION", string.Empty).Replace(":", string.Empty).Trim() + ".", string.Empty)
                                        .Trim()
                                    ),
                                    OfficialName = s.Descendants("span").FirstOrDefault().InnerText.Trim()
                                };

                                // Ret data
                                return breedSubSection;
                            })
                            .ToList()
                    };

                    // Clear data
                    breedSections.SubSections.ForEach(fe => fe.OfficialName = fe.OfficialName.Replace(breedSections.Index + "." + fe.Index, string.Empty).Trim());

                    // Ret data
                    return breedSections;
                })
                .ToList();

            // return data
            return listBreedSections;
        }

        /// <summary>
        /// Private method that extract the list of breed detail pages of definitive breeds
        /// </summary>
        /// <returns></returns>
        private List<string> ExtractListOfDetailsPagesOfDefinitiveBreed()
        {
            // Check cache data
            if (_cacheData && _listDetailPagesDefinitivelBreeds != null) return _listDetailPagesDefinitivelBreeds;

            // Init var
            List<string> listDetailPagesDefinitiveBreeds = new List<string>();

            // Extract groups
            List<BreedGroup> listBreedGroups = GetBreedGroups();

            // Iterate group detail pages and extract uri to breed detail page
            listBreedGroups.ForEach(breedGroup =>
            {
                // Download page
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(breedGroup.DetailPage);

                // Add detail pages
                listDetailPagesDefinitiveBreeds.AddRange(
                    doc.DocumentNode
                        .Descendants("a")
                        .Where(w => w.HasClass("nom"))
                        .Select(s => FCI_BASE_URI + s.GetAttributeValue<string>("href", string.Empty))
                );
            });
            
            // Cache data
            if (_cacheData)
            {
                _listDetailPagesDefinitivelBreeds = listDetailPagesDefinitiveBreeds;
            }

            // Return data
            return listDetailPagesDefinitiveBreeds;
        }

        /// <summary>
        /// Private method that extract the list of breed detail pages of provisional breeds
        /// </summary>
        /// <returns></returns>
        private List<string> ExtractListOfDetailsPagesOfProvisionalBreed()
        {
            // Check cache data
            if (_cacheData && _listDetailPagesProvisionalBreeds != null) return _listDetailPagesProvisionalBreeds;

            // Init var
            List<string> listDetailPagesProvisionalBreeds = new List<string>();

            // Download html page and parse it
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(FCI_URI_PROVISIONAL_PAGE);

            // Get list with uri detail page
            listDetailPagesProvisionalBreeds = doc.DocumentNode
                .Descendants("a")
                .Where(w => w.HasClass("nom"))
                .Select(s => FCI_BASE_URI + s.GetAttributeValue<string>("href", string.Empty))
                .ToList();

            // Cache data
            if (_cacheData)
            {
                _listDetailPagesProvisionalBreeds = listDetailPagesProvisionalBreeds;
            }

            // return data
            return listDetailPagesProvisionalBreeds;
        }

        /// <summary>
        /// Private method that extract all data from breed detail page
        /// </summary>
        /// <param name="uriDetailPage"></param>
        /// <returns></returns>
        private Breed ExtractDataFromBreedDetailPage(string uriDetailPage)
        {
            // Init var
            Breed breed = new Breed();

            // Get groups
            List<BreedGroup> listBreedGroups = GetBreedGroups();

            // Get sections
            List<BreedSection> listBreedSections = GetBreedSections();

            // Download page
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(uriDetailPage);

            // Get detail page
            breed.DetailPage = uriDetailPage;

            // Get code
            breed.Code = int.Parse(doc.DocumentNode.Descendants("h2").FirstOrDefault(w => w.HasClass("nom")).Descendants("span").ToList()[1].InnerText.Trim());

            // Get official name
            breed.OfficialName = doc.DocumentNode.Descendants("h2").FirstOrDefault(w => w.HasClass("nom")).Descendants("span").ToList()[0].InnerText.Trim();

            // Get group
            string groupIndex = doc.DocumentNode.Descendants("span").ToList().Find(f => f.InnerText == "Group :")?.ParentNode.Element("a")?.InnerText;
            groupIndex = groupIndex.Replace("n°", string.Empty);
            groupIndex = groupIndex.Substring(0, groupIndex.IndexOf(' ')).Trim();
            breed.Group = listBreedGroups.Find(f => f.Index == int.Parse(groupIndex));

            // Get translations
            breed.OfficialNameTranslations = doc.DocumentNode.Descendants("table").FirstOrDefault(w => w.HasClass("racesgridview"))
                .Elements("tr")
                .Skip(1)    // Remove header row
                .Select(s => new Tuple<string, string>(
                        item1: GetIsoCodeLang(s.Elements("td").ToList()[0].InnerText.Trim()),
                        item2: s.Elements("td").ToList()[1].Element("span").InnerText.Trim()
                    )
                )
                .ToList();

            // Get pubblications
            breed.Pubblications = doc.DocumentNode.Descendants("table").First(w => w.HasClass("racesgridview"))
                .Elements("tr")
                .Skip(1)    // Remove header row
                .Where(w => w.Elements("td").ToList()[2].Element("span").InnerText.Trim() != "-")
                .Select(s => new Tuple<string, DateTime, string>(
                        item1: GetIsoCodeLang(s.Elements("td").ToList()[0].InnerText.Trim()),
                        item2: DateTime.ParseExact(s.Elements("td").ToList()[2].Element("span").InnerText.Trim(), "M/d/yyyy", CultureInfo.InvariantCulture),
                        item3: FCI_BASE_URI + "/" + s.Elements("td").ToList()[3].Element("a").GetAttributeValue<string>("href", string.Empty).Replace("../", string.Empty)
                    )
                )
                .ToList();

            // Get data from info table
            doc.DocumentNode
                .Descendants("table")
                .Where(w => w.HasClass("racetable"))
                .SelectMany(s => s.Elements("tr"))
                .ToList()
                .ForEach(trInfo =>
                {
                    string entryName = trInfo.Elements("td").ToList()[0].Element("span").InnerText.Trim();
                    string value = trInfo.Elements("td").ToList()[1].Element("span").InnerText.Trim();

                    switch (entryName)
                    {
                        // Get section
                        case "Section":
                            breed.Section = listBreedSections.Find(f => f.OfficialName == value);
                            break;

                        // Get sub section
                        case "Subsection":
                            breed.SubSection = breed.Section.SubSections.Find(f => f.OfficialName == value);
                            break;

                        // Get date of acceptance on provisional basis by the FCI
                        case "Date of acceptance on a provisional basis by the FCI":
                            breed.DateOfAcceptanceOnProvisionalBasisByTheFci = DateTime.ParseExact(value, "M/d/yyyy", CultureInfo.InvariantCulture);
                            break;

                        // Get official name language
                        case "Official authentic language":
                            breed.IsoOfficialLang = GetIsoCodeLang(value);
                            break;

                        // Get date official publication
                        case "Date of publication of the official valid standard":
                            breed.DateOfPubblicationOfTheOfficialValidStandard = DateTime.ParseExact(value, "M/d/yyyy", CultureInfo.InvariantCulture);
                            break;

                        // Get status
                        case "Breed status":
                            switch (value)
                            {
                                // Provisional
                                case "Recognized on a provisional basis":
                                    breed.Status = BreedStatus.Provisional;
                                    break;

                                // Definitive
                                case "Recognized on a definitive basis":
                                    breed.Status = BreedStatus.Definitive;
                                    break;

                                // Not implemented
                                default:
                                    throw new NotImplementedException(
                                        string.Format(
                                            "Warning! entry of breed status equals to '{0}' not supported yet.",
                                            value
                                        )
                                    );
                            }
                            break;

                        // Get origin country
                        case "Country of origin of the breed":
                            breed.OriginCountries = value.Split(',').Where(w => !string.IsNullOrEmpty(w) && !string.IsNullOrEmpty(w.Trim())).ToArray();
                            break;

                        // Get working trial
                        case "Working trial":
                            switch (value)
                            {
                                // Not subject 
                                case "Not subject to a working trial according to the FCI breeds nomenclature":
                                    breed.WorkingTrial = BreedWorkingTrial.NotSubject;
                                    break;

                                // Subject 
                                case "Subject to a working trial according to the FCI Breeds Nomenclature":
                                    breed.WorkingTrial = BreedWorkingTrial.Subject;
                                    break;

                                // Subject limited on country applied
                                case "Subject to a working trial only for the countries having applied for it":
                                    breed.WorkingTrial = BreedWorkingTrial.SubjectLimitedOnCountryApplied;
                                    break;

                                // Subject on some countries
                                case "Subject to a working trial for some countries":
                                    breed.WorkingTrial = BreedWorkingTrial.SubjectOnSomeCountry;
                                    break;

                                // Subject to nordic countries
                                case "Subject to a working trial only for the Nordic countries (Finland, Norway, Sweden)":
                                    breed.WorkingTrial = BreedWorkingTrial.SubjectOnNordicCountries;
                                    break;

                                // Not implemented
                                default:
                                    throw new NotImplementedException(
                                        string.Format(
                                            "Warning! entry of breed working trial equals to '{0}' not supported yet.",
                                            value
                                        )
                                    );
                            }
                            break;

                        // Get patronage country
                        case "Country of patronage of the breed":
                            breed.PatronageCountries = value.Split(',').Where(w => !string.IsNullOrEmpty(w) && !string.IsNullOrEmpty(w.Trim())).ToArray();
                            break;

                        // Get date acceptance
                        case "Date of acceptance on a definitive basis by the FCI":                            
                            breed.DateOfAcceptanceOnDefinitiveBasisByTheFci = DateTime.ParseExact(value, "M/d/yyyy", CultureInfo.InvariantCulture);
                            break;
                        
                        // Get development country
                        case "Country of development of the breed":
                            breed.DevelopmentCountries = value.Split(',').Where(w => !string.IsNullOrEmpty(w) && !string.IsNullOrEmpty(w.Trim())).ToArray();
                            break;

                        // Not implemented
                        default:
                            throw new NotImplementedException(
                                string.Format(
                                    "Warning! entry of info table equals to '{0}' not supported yet.",
                                    entryName
                                )
                            );
                    }
                });

            // Get varieties
            breed.Varieties = new List<BreedVariety>();

            doc.DocumentNode.Descendants("div").FirstOrDefault(w => w.HasClass("varietes"))?
                .Elements("tr")
                .Skip(1)    // Remove header row
                .ToList()
                .ForEach(variety =>
                {
                    var findSubVarieties = variety.Descendants("div").ToList().Find(f => f.HasClass("sousvarietes"));

                    if (findSubVarieties == null)
                    {
                        // Variety
                        BreedVariety breedVariety = new BreedVariety()
                        {
                            IndexLetter = variety.Elements("td").ToList()[0].Element("span").InnerText.Trim(),
                            OfficialName = variety.Elements("td").ToList()[0].Element("span").InnerText.Trim(),
                            Cacib = variety.Elements("td").ToList()[1].Element("span")?.InnerText.Trim() == "*"
                        };

                        // Correct letter & official name
                        breedVariety.IndexLetter = breedVariety.IndexLetter.Substring(0, breedVariety.IndexLetter.IndexOf(')')).Trim();
                        breedVariety.OfficialName = breedVariety.OfficialName.Substring(breedVariety.OfficialName.IndexOf(')') + 1).Trim();

                        breed.Varieties.Add(breedVariety);
                    }
                    else
                    {
                        // Sub-variety
                        breed.Varieties.Last().SubVarieties = findSubVarieties
                                .Descendants("tr")
                                .Select(trSubVariety => new BreedSubVariety()
                                {
                                    OfficialNames = trSubVariety.Elements("td").ToList()[0].Element("span").InnerHtml.Trim()
                                        .Split("<br>").Where(w => !string.IsNullOrEmpty(w)).ToArray(),
                                    Cacib = trSubVariety.Elements("td").ToList()[1].Element("span")?.InnerText == "*",
                                })
                                .ToList();
                    }
                });

            // Get illustrations
            breed.ListImages = doc.DocumentNode.Descendants("ul").FirstOrDefault(w => w.HasClass("illustrations"))
                .Elements("li")
                .Reverse().Skip(1).Reverse()    // Remove basic anatomy image (always last)
                .Select(s => FCI_BASE_URI + s.Element("a").GetAttributeValue<string>("href", string.Empty))
                .ToList();

            // Get educations
            breed.EducationResources = doc.DocumentNode.Descendants("ul").FirstOrDefault(w => w.HasClass("education"))?
                .Elements("li")
                .Select(s => 
                {
                    Uri uri = new Uri(FCI_BASE_URI + "/" + s.Element("a").GetAttributeValue<string>("href", string.Empty).Replace("../", string.Empty));
                    
                    // Check .flv uri of video
                    if (!string.IsNullOrEmpty(uri.Query) && !string.IsNullOrEmpty(uri.Query.Trim()))
                    {
                        NameValueCollection queryParams = HttpUtility.ParseQueryString(uri.Query);
                        string urlParam = queryParams["url"];
                        if (!string.IsNullOrEmpty(urlParam))
                        {
                            uri = new Uri(FCI_BASE_URI + urlParam);
                        }
                    }

                    // Extract info form education resource
                    string fileType = string.Empty;
                    string extension = Path.GetExtension(uri.ToString());                    
                    switch (extension)
                    {
                        // Flash video
                        case ".flv":
                            fileType = "flash-video";
                            break;

                        // Video
                        case ".mp4":
                        case ".mov":
                            fileType = "video";
                            break;

                        // Pdf
                        case ".pdf":
                            fileType = "pdf";
                            break;

                        // Power point
                        case ".ppt":
                        case ".pptx":
                        case ".pptm":
                        case ".ppsx":
                            fileType = "powerpoint";
                            break;

                        // Not implemented
                        default:
                            throw new NotImplementedException(
                                string.Format(
                                    "Warning! entry of extension equals to '{0}' not supported yet.",
                                    extension
                                )
                            );
                    }

                    // Return data
                    return new Tuple<string, string, string>(extension, fileType, uri.ToString());
                })
                .ToList();

            // Return data
            return breed;
        }

        /// <summary>
        /// Private method that extract data of Cabib at breeds level
        /// </summary>
        /// <param name="listBreeds"></param>
        /// <returns></returns>
        private List<Breed> GetBreedCabibData(List<Breed> listBreeds)
        {
            // Extract the breed groups
            List<BreedGroup> listBreedGroups = GetBreedGroups();

            // Iterate detail pages of breed groups
            listBreedGroups.ForEach(breedGroup =>
            {
                // Download page
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(breedGroup.DetailPage);

                // Extract breeds with cabib and set it into list breeds
                doc.DocumentNode
                    .Descendants("td")
                    .Where(w => w.HasClass("race"))
                    .ToList()
                    .ForEach(breedTd =>
                    {
                        string breedCode = breedTd.Elements("a").First().GetAttributeValue<string>("name", string.Empty);
                        if (!string.IsNullOrEmpty(breedCode))
                        {
                            var findBreed = listBreeds.Find(f => f.Code == int.Parse(breedCode));
                            if (findBreed != null)
                            {
                                findBreed.Cacib = breedTd.ParentNode.Descendants("td").FirstOrDefault(f => f.HasClass("racecabib"))?.Element("span")?.InnerText.Trim() == "*";
                            }
                            else
                            {
                                throw new ArgumentException(
                                    string.Format(
                                        "Warning! Param 'breedCode' equals to '{0}' not found in breeds list",
                                        breedCode
                                    )
                                );
                            }
                        }
                        else
                        {
                            throw new ArgumentException(
                                string.Format(
                                    "Warning! Param 'breedCode' is equals to null or empty"
                                )
                            );
                        }
                    });
            });

            // Return data
            return listBreeds;
        }

        #endregion

    }

}
