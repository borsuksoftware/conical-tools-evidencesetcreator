using System.Linq;
using System.Collections.Generic;

using Client = BorsukSoftware.Conical.Client;

namespace BorsukSoftware.Conical.Tools.EvidenceSetsCreator
{
    public class Program
    {
        const string CONST_HELPTEXT = @"Conical evidence sets creator tool
==================================

This tool is designed to make it easy to create evidence sets within a Conical instance 
based off of arbitrary search criteria. Typical use cases of the tool include CI processes
where, after a series of individual test run sets have been uploaded, they need to be collated
together for analysis.

The tool works by allowing a user to supply a set of search criteria which define the TSRs to
be included in the created evidence set.

The following parameters are available:

General:

 -server [url]            The server to connect to
 -token [tokenValue]      The token to use (optional)

Evidence Set General:

 -product XXX           the name of the product containing the evidence set
 -name XXX              The name of the evidence set
 -description XXX       The description of the evidence set (optional)
 -tag XXX               A tag to add (can be specified multiple times)
 -refdate XXX           The ref date to use for the evidence set (optional)
 -refdateformat XXX     The .net format string to use when parsing the ref date if required (optional)

 -link name url description     Details of a link to apply to the created ES

 -multipleSourceTestRunsBehaviour mode      Desired behaviour when multiple test runs contribute to a single test
    - NotAllowed
    - UseBestResult
    - UseWorstResult
    - UseLastResult
    - UseFirstResult

Test Run Set Specification

 -searchCriteriaCount XXX           The number of search criterias which are specified
 -searchCriteria idx name value     Specify a search criteria value. 

The following values are valid

    prefix XXX              Specify the prefix to be used for these test run sets

    product XXX             Specify the product to be included in the search (multiple are allowed)
    status XXX              Specify the TRS status to be searched (multiple)
    name XXX                Specify the name criteria
    description XXX         Specify the description criteria
    creator XXX             Specify the creator criteria
    tag XXX                 Specify the required tag (multiple)

    minRefDate XXX          Specify the minRefDate
    minRefDateFormat XXX    Specify the minRefDate format
    maxRefDate XXX          Specify the maxRefDate
    maxRefDateFormat XXX    Specify the maxRefDate format

    minRunDate XXX          Specify the munRefDate
    minRunDateFormat XXX    Specify the munRefDate format
    maxRunDate XXX          Specify the munRefDate
    maxRunDateFormat XXX    Specify the munRefDate format
    
";
        public static async Task<int> Main(string[] args)
        {
            string? server = null, accessToken = null, targetProduct = null;
            string? targetName = null, targetDescription = null;
            string? targetRefDateStr = null, targetRefDateFormatStr = null;
            var targetTags = new List<string>();
            var targetExternalLinks = new List<(string Name, string Url, string Description)>();

            Client.EvidenceSetTestMultipleSourceTestRunsBehaviour evidenceSetTestMultipleSourceTestRunsBehaviour = Client.EvidenceSetTestMultipleSourceTestRunsBehaviour.NotAllowed;

            int? searchCriteriaCount = null;
            var searchCriteria = new Dictionary<int, TestRunSetSearchCriteriaModel>();

            if (args.Length == 0)
            {
                Console.WriteLine(CONST_HELPTEXT);
                return 0;
            }

            for (int argIdx = 0; argIdx < args.Length; ++argIdx)
            {
                switch (args[argIdx].ToLower())
                {
                    case "-help":
                    case "-?":
                        Console.WriteLine(CONST_HELPTEXT);
                        return 0;

                    case "-server":
                        server = args[++argIdx];
                        break;

                    case "-token":
                        accessToken = args[++argIdx];
                        break;

                    case "-product":
                        targetProduct = args[++argIdx];
                        break;

                    case "-name":
                        targetName = args[++argIdx];
                        break;

                    case "-description":
                        targetDescription = args[++argIdx];
                        break;

                    case "-tag":
                        targetTags.Add(args[++argIdx]);
                        break;

                    case "-refdate":
                        targetRefDateStr = args[++argIdx];
                        break;

                    case "-refdateformat":
                        targetRefDateFormatStr = args[++argIdx];
                        break;

                    case "-link":
                        {
                            var linkName = args[++argIdx];
                            var linkUrl = args[++argIdx];
                            var linkDescription = args[++argIdx];

                            targetExternalLinks.Add((Name: linkName, Url: linkUrl, Description: linkDescription));
                            break;
                        }

                    case "-multiplesourcetestrunsbehaviour":
                        {
                            var str = args[++argIdx];
                            if (!Enum.TryParse<Client.EvidenceSetTestMultipleSourceTestRunsBehaviour>(str, true, out var result))
                            {
                                Console.WriteLine($"Unable to parse '{str}' as a valid option for multiple source test runs behaviour");
                                return 1;
                            }

                            evidenceSetTestMultipleSourceTestRunsBehaviour = result;
                            break;
                        }


                    case "-searchcriteriacount":
                        {
                            var searchCriteriaCountStr = args[++argIdx];
                            if (!int.TryParse(searchCriteriaCountStr, out var searchCriteriaCountVal))
                            {
                                Console.WriteLine($"Unable to parse '{searchCriteriaCountStr}' as a valid search criteria count");
                                return 1;
                            }
                            searchCriteriaCount = searchCriteriaCountVal;
                            break;
                        }

                    case "-searchcriteria":
                        {
                            string idxStr = args[++argIdx];
                            string criteriaName = args[++argIdx];
                            string criteriaValue = args[++argIdx];

                            if (!int.TryParse(idxStr, out var idx))
                            {
                                Console.WriteLine($"Unable to parse '{idxStr}' as a valid idx for search criteria");
                                return 1;
                            }

                            if (!searchCriteria.TryGetValue(idx, out var testRunSetSearchCriteriaModel))
                            {
                                testRunSetSearchCriteriaModel = new TestRunSetSearchCriteriaModel();
                                searchCriteria[idx] = testRunSetSearchCriteriaModel;
                            }

                            switch (criteriaName.ToLower())
                            {
                                case "prefix":
                                    testRunSetSearchCriteriaModel.Prefix = criteriaValue;
                                    break;

                                case "product":
                                    testRunSetSearchCriteriaModel.Products.Add(criteriaValue);
                                    break;

                                case "status":
                                    {
                                        if (!Enum.TryParse<Client.TestRunSetStatus>(criteriaValue, out var status))
                                        {
                                            Console.WriteLine($"Unable to parse '{criteriaValue}' as a valid test run set status");
                                            return 1;
                                        }

                                        testRunSetSearchCriteriaModel.Statuses.Add(status);
                                        break;
                                    }

                                case "name":
                                    testRunSetSearchCriteriaModel.Name = criteriaValue;
                                    break;

                                case "description":
                                    testRunSetSearchCriteriaModel.Description = criteriaValue;
                                    break;

                                case "creator":
                                    testRunSetSearchCriteriaModel.Creator = criteriaValue;
                                    break;

                                case "tag":
                                    testRunSetSearchCriteriaModel.Tags.Add(criteriaValue);
                                    break;

                                case "minrefdate":
                                    testRunSetSearchCriteriaModel.MinRefDate = criteriaValue;
                                    break;

                                case "minrefdateformat":
                                    testRunSetSearchCriteriaModel.MinRefDateFormat = criteriaValue;
                                    break;

                                case "maxrefdate":
                                    testRunSetSearchCriteriaModel.MaxRefDate = criteriaValue;
                                    break;

                                case "maxrefdateformat":
                                    testRunSetSearchCriteriaModel.MaxRefDateFormat = criteriaValue;
                                    break;

                                case "minrundate":
                                    testRunSetSearchCriteriaModel.MinRunDate = criteriaValue;
                                    break;

                                case "minrundateformat":
                                    testRunSetSearchCriteriaModel.MinRunDateFormat = criteriaValue;
                                    break;

                                case "maxrundate":
                                    testRunSetSearchCriteriaModel.MaxRefDate = criteriaValue;
                                    break;

                                case "maxrundateformat":
                                    testRunSetSearchCriteriaModel.MaxRefDateFormat = criteriaValue;
                                    break;

                                default:
                                    Console.WriteLine($"Unknown search criteria name '{criteriaName}' found for idx #{idx}");
                                    return 1;
                            }
                            break;
                        }

                    default:
                        {
                            Console.WriteLine($"Unknown command line param at index {argIdx} - {args[argIdx]}");
                            return 1;
                        }
                }
            }

            Console.WriteLine("Conical evidence set creator");

            if (string.IsNullOrEmpty(server))
            {
                Console.WriteLine("No server specified - terminating");
                return 1;
            }

            if (!Uri.TryCreate(server, UriKind.Absolute, out var _))
            {
                Console.WriteLine($"Unable to parse {server} as a valid Uri");
                return 1;
            }

            if (string.IsNullOrEmpty(targetProduct))
            {
                Console.WriteLine("No target product specified - terminating");
                return 1;
            }

            var apiService = new Client.REST.AccessLayer(server, accessToken);

            Client.IProduct product;
            try
            {
                product = await apiService.GetProduct(targetProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught trying to access '{targetProduct}': {ex}");
                return 1;
            }

            if (!searchCriteriaCount.HasValue)
            {
                searchCriteriaCount = searchCriteria.Count == 0 ?
                    0 :
                    searchCriteria.Max(pair => pair.Key);
            }

            var toProcess = Enumerable.Range(0, searchCriteriaCount.Value).
                Select(idx =>
                {
                    searchCriteria.TryGetValue(idx, out var criteria);

                    return new { idx, criteria };
                });

            var missingCriteria = toProcess.Where(t => t.criteria == null);
            var missingCriteriaEnumerator = missingCriteria.GetEnumerator();
            if (missingCriteriaEnumerator.MoveNext())
            {
                Console.WriteLine($"Search criteria #{missingCriteriaEnumerator.Current.idx} doesn't exist, expected {searchCriteriaCount} criteria");
                return 1;
            }

            var rawResults = new List<(string? Prefix, IReadOnlyCollection<Client.ITestRunSetSummary> TestRunSets)>();
            foreach (var tuple in toProcess)
            {
                try
                {
                    DateTime? minRefDate = null, maxRefDate = null, minRunDate = null, maxRunDate = null;

                    if (!string.IsNullOrEmpty(tuple.criteria.MinRefDate))
                    {
                        if (string.IsNullOrEmpty(tuple.criteria.MinRefDateFormat))
                        {
                            if (!DateTime.TryParse(tuple.criteria.MinRefDate, out var date))
                            {
                                Console.WriteLine($"Unable to parse '{tuple.criteria.MinRefDate}' as a valid min ref date for search criteria idx #{tuple.idx}");
                                return 1;
                            }

                            minRefDate = date;
                        }
                        else
                        {
                            if (!DateTime.TryParseExact(tuple.criteria.MinRefDate, tuple.criteria.MinRefDateFormat, null, System.Globalization.DateTimeStyles.AssumeUniversal, out var date))
                            {
                                Console.WriteLine($"Unable to parse '{tuple.criteria.MinRefDate}' using format '{tuple.criteria.MinRefDateFormat}' as a valid min ref date for search criteria idx #{tuple.idx}");
                                return 1;
                            }

                            minRefDate = date;
                        }
                    }

                    if (!string.IsNullOrEmpty(tuple.criteria.MaxRefDate))
                    {
                        if (string.IsNullOrEmpty(tuple.criteria.MaxRefDateFormat))
                        {
                            if (!DateTime.TryParse(tuple.criteria.MaxRefDate, out var date))
                            {
                                Console.WriteLine($"Unable to parse '{tuple.criteria.MaxRefDate}' as a valid max ref date for search criteria idx #{tuple.idx}");
                                return 1;
                            }

                            maxRefDate = date;
                        }
                        else
                        {
                            if (!DateTime.TryParseExact(tuple.criteria.MaxRefDate, tuple.criteria.MaxRefDateFormat, null, System.Globalization.DateTimeStyles.AssumeUniversal, out var date))
                            {
                                Console.WriteLine($"Unable to parse '{tuple.criteria.MaxRefDate}' using format '{tuple.criteria.MaxRefDateFormat}' as a valid max ref date for search criteria idx #{tuple.idx}");
                                return 1;
                            }

                            maxRefDate = date;
                        }
                    }

                    if (!string.IsNullOrEmpty(tuple.criteria.MinRunDate))
                    {
                        if (string.IsNullOrEmpty(tuple.criteria.MinRunDateFormat))
                        {
                            if (!DateTime.TryParse(tuple.criteria.MinRunDate, out var date))
                            {
                                Console.WriteLine($"Unable to parse '{tuple.criteria.MinRunDate}' as a valid min run date for search criteria idx #{tuple.idx}");
                                return 1;
                            }

                            minRunDate = date;
                        }
                        else
                        {
                            if (!DateTime.TryParseExact(tuple.criteria.MinRunDate, tuple.criteria.MinRunDateFormat, null, System.Globalization.DateTimeStyles.AssumeUniversal, out var date))
                            {
                                Console.WriteLine($"Unable to parse '{tuple.criteria.MinRunDate}' using format '{tuple.criteria.MinRunDateFormat}' as a valid min run date for search criteria idx #{tuple.idx}");
                                return 1;
                            }

                            minRunDate = date;
                        }
                    }

                    if (!string.IsNullOrEmpty(tuple.criteria.MaxRunDate))
                    {
                        if (string.IsNullOrEmpty(tuple.criteria.MaxRunDateFormat))
                        {
                            if (!DateTime.TryParse(tuple.criteria.MaxRunDate, out var date))
                            {
                                Console.WriteLine($"Unable to parse '{tuple.criteria.MaxRunDate}' as a valid max run date for search criteria idx #{tuple.idx}");
                                return 1;
                            }

                            maxRunDate = date;
                        }
                        else
                        {
                            if (!DateTime.TryParseExact(tuple.criteria.MaxRunDate, tuple.criteria.MaxRunDateFormat, null, System.Globalization.DateTimeStyles.AssumeUniversal, out var date))
                            {
                                Console.WriteLine($"Unable to parse '{tuple.criteria.MaxRunDate}' using format '{tuple.criteria.MaxRunDateFormat}' as a valid max run date for search criteria idx #{tuple.idx}");
                                return 1;
                            }

                            maxRunDate = date;
                        }
                    }

                    Console.WriteLine($"Searching TRS set #{tuple.idx}");

                    var searchResults = await apiService.SearchTestRunSets(
                        tuple.criteria.Products,
                        tuple.criteria.Statuses,
                        tuple.criteria.Name,
                        tuple.criteria.Description,
                        tuple.criteria.Creator,
                        minRefDate,
                        maxRefDate,
                        minRunDate,
                        maxRunDate,
                        tuple.criteria.Tags);

                    Console.WriteLine($" => {searchResults.Results.Count} results");

                    rawResults.Add((tuple.criteria.Prefix, searchResults.Results));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception caught processing search criteria #{tuple.idx} - {ex}");
                    return 1;
                }
            }

            Console.WriteLine();
            var sources = rawResults.
                SelectMany(tuple => tuple.TestRunSets.Select(trs => new { tuple.Prefix, trs })).
                Select(tuple => (Prefix: tuple.Prefix, Product: tuple.trs.Product, TestRunSetID: tuple.trs.ID, TestRunSelectionMode: Client.EvidenceSetTestRunSelectionMode.All, TestRunIDs: (IReadOnlyCollection<int>?)null)).
                ToList();

            try
            {
                DateTime? targetRefDate = null;

                if (!string.IsNullOrEmpty(targetRefDateStr))
                {
                    targetRefDate = !string.IsNullOrEmpty(targetRefDateFormatStr) ?
                        DateTime.ParseExact(targetRefDateStr, targetRefDateFormatStr, null) :
                        DateTime.Parse(targetRefDateStr);
                }

                Console.WriteLine($"Creating evidence set");
                var evidenceSet = await product.CreateEvidenceSet(targetName,
                    targetDescription,
                    targetRefDate,
                    targetTags,
                    targetExternalLinks,
                    evidenceSetTestMultipleSourceTestRunsBehaviour,
                    sources);

                Console.WriteLine($"Created - #{evidenceSet.ID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" => failed ({ex})");
                return 1;
            }

            return 0;
        }
    }

    public class TestRunSetSearchCriteriaModel
    {
        public string? Prefix { get; set; }

        public HashSet<string> Products = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<Client.TestRunSetStatus> Statuses = new HashSet<Client.TestRunSetStatus>();
        public string? Name;
        public string? Description;
        public string? Creator;

        public string? MinRefDate;
        public string? MinRefDateFormat;
        public string? MaxRefDate;
        public string? MaxRefDateFormat;

        public string? MinRunDate;
        public string? MinRunDateFormat;
        public string? MaxRunDate;
        public string? MaxRunDateFormat;

        public HashSet<string> Tags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
    }
}