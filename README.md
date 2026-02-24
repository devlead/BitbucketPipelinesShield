# Bitbucket Pipelines Shield

Azure Function that provides a shield (badge) for your Bitbucket Pipelines build status.

[![Build using Cake.Sdk and File based Cake](https://github.com/devlead/BitbucketPipelinesShield/actions/workflows/CakeSdk.yml/badge.svg)](https://github.com/devlead/BitbucketPipelinesShield/actions/workflows/CakeSdk.yml)

[![Deploy to Azure](http://azuredeploy.net/deploybutton.svg)](https://azuredeploy.net/)

## Usage

The app serves an SVG badge for the latest build status and can redirect to the latest build log.

### Status

Returns an SVG badge for the given repo and branch (or commit node).

```
{baseUrl}/status/{owner}/{repo}/{node}
```

### Url

Redirects to the latest pipeline build log for the given repo and branch.

```
{baseUrl}/url/{owner}/{repo}/{node}
```

Replace `{baseUrl}` with the URL where the function app is deployed (e.g. `https://your-function-app.azurewebsites.net`). `{owner}` and `{repo}` are the Bitbucket workspace and repository; `{node}` is the branch name or commit hash.

### Markdown

```markdown
[![Build Status]({baseUrl}/status/{owner}/{repo}/{node})]({baseUrl}/url/{owner}/{repo}/{node})
```

## Limitations

Only public repositories are supported. The project is an early proof of concept and may not work for every repo.
