This repository contains the source code for a .net command line tool for creating evidence sets for Conical.

For more details on Conical, please see the main website - https://conical.cloud

## Purpose
The tool exists to make it easier to manage the CI workflow by providing a general purpose command line based evidence set creation tool which can be used across multiple use cases without the need for custom apps.

## Usage
The tool works under the assumption that for each CI job, there are a set of search criteria which can identify the set of test run sets which should form part of the evidence set. These criteria can use any of the usual search features and have an optional tests prefix.

Note that the typical recommendation here is to use the tags functionality when uploading data and then searching by these.

Full details of the supported options can be found by running the tool with no arguments or by passing in -help.

#### Example usage
Within our internal usage, every TRS created by a given CI run will have a specific tag associated with it, e.g. 'ci-119' for build job #119. Within that CI run, each different test run set that is created is then tagged with additional tags describing the TRS.

A worked example would be when we're testing the underlying docker image prior to release, we not only have a set of automated regression tests which check that the various upgrades / installers work (the results uploaded with the tags 'ci-%build.number%' and 'deployment') but we also re-run the set of regression tests at the underlying API level (which are subsequently tagged with 'ci-%build.number%' and 'api' when uploaded). When we create the evidence set which combines these together, we would like the api tests to be prefixed with api and the deployment ones with deployment. This is achieved by using 2 different search criteria based off of tags and then assigning a prefix to each group, e.g.

Set 0:
 - prefix = api
 - tag = ci-%build.number%
 - tag = api
 
Set 1:
 - prefix = deployment
 - tag = ci-%build.number%
 - tag = deployment
 
This approach allows us to automate everything.

## FAQs
#### We've found a bug, what do we do?
Contact us / raise an issue / raise a PR with the fix.

#### Our use-case is slightly different, what should we do?
Short answer - Get in touch with us. 
Long answer - If your use-case can be added to the tool and be used by other people, then we'll see what we can do. If it's more specialised, then we'd recommend copying the existing project and make your changes under a different tool name.

#### I have suggestions for improvements, what do I do?
Get in touch with us either via our website or raise an issue in the project