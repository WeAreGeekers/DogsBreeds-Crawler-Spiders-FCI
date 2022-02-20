using WeAreGeekers.DogsBreeds.Crawler.Spiders.FCI;
using WeAreGeekers.DogsBreeds.Crawler.Spiders.FCI.Responses;

// Create object
FCISpider fciSpider = new FCISpider(
    cacheData: true // Set true if you want to cache data into internal list to avoid multiple http call to same uri
);

// Extract FCI Breed Groups
List<BreedGroup> listBreedGroups = fciSpider.GetBreedGroups();

// Extract FCI Breed Sections
List<BreedSection> listBreedSections = fciSpider.GetBreedSections();

// Extract FCI Breeds (All)
List<Breed> listBreeds = fciSpider.GetBreeds();

// Extract FCI Breeds (Only definitive breeds)
List<Breed> listDefinitiveBreeds = fciSpider.GetDefinitiveBreeds();

// Extract FCI Breeds (Only provisional breeds)
List<Breed> listProvisionalBreeds = fciSpider.GetProvisionalBreeds();

// Clear internal cache of spider
fciSpider.ClearCache();
