# Bitbucket Pipelines Shield
NancyFX based project that provides a shield for your Bitbucket Pipelines build status


| Build server                | Platform     | Status                                                                                                                    |
|-----------------------------|--------------|---------------------------------------------------------------------------------------------------------------------------|
| Bitbucket Pipelines         | Linux        | [![Build Status](https://cakebitbucketpipelinesshield.azurewebsites.net/status/devlead/bitbucketpipelinesshield/master)](https://bitbucketshield.azurewebsites.net/url/devlead/bitbucketpipelinesshield/master) |
| AppVeyor                    | Windows      | [![Build Status](https://img.shields.io/appveyor/ci/devlead/BitbucketPipelinesShield/master.svg)](https://ci.appveyor.com/project/devlead/bitbucketpipelinesshield/branch/master) |
| TravisCI                    | Linux & OSX  | [![Build Status](https://travis-ci.org/devlead/BitbucketPipelinesShield.svg?branch=master)](https://travis-ci.org/devlead/BitbucketPipelinesShield) |

[![Deploy to Azure](http://azuredeploy.net/deploybutton.svg)](https://azuredeploy.net/)

## Usage
The web site can provide an SVG for last build status and also redirect to latest build log.

### Status
The status route provides an shields.io badge for given repo and branch.
```
{url where shield project is deployed}/[site]/status/{owner}/{repo}/{node}
```
### Url
The url route provides and redirect to latest build log for given repo and branch.
```
{url where shield project is deployed}/url/{owner}/{repo}/{node}
```
### Markdown
```
[![Build Status]](status url)](redirect url)
```

## Limitations
Currently only supports public repositories, the project is at an rough initial proof of concept stage and might not work at all for your repo.
