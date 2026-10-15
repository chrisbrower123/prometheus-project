# Prompt log

This log records the AI-assisted development work for the take-home assessment. Generated code was reviewed against the requirements, compiled, tested, and adjusted where needed.

## Issue 1. Backend Scaffolding and Setup

**Prompt used:** (Provided project brief). I want to begin scaffolding my .NET backend. I have already created a web api project, and we need to identify necessary models, abstractions, providers, etc. for the project.

Right now, I am thinking we will need a src/ and a test/

For src,
I'm thinking we will need contracts for the aggregation engine, the provider of market data, and our service to take a request and give a response.

Our domain models for this seem like they would be a stock, a daily summary, and a collection of daily summaries.

We will need exceptions for these models, help me identify them.

To make this work, we will need a providers folder that defines our specific yahoo configurations.

What are your thoughts on this, additions/exclusions, etc.

Focus your response on necessary domain entities and project structure rather than any code implementations.

**Why:** I wanted to double check my original ideas for architecture, see what I was missing, and what I could think about doing instead.

**Review:** Claude suggested I break the src/ folder into three separate .NET projects, one for domain and contracts, one for providers, and one specifically for the api. It also suggested I drop Stock as an entity, and add IntradayBar. It approved my ideas for providers and other domain models. It also added clarification that market time grouping can be sneaky, and identified essential error types to be used. I will make the necessary scaffolding now.


**Prompt Used:** Ok, using everything we just discussed about planned architecture, make an implementation spec for a Codex agent that scaffolds our three .NET projects: Core, Infrastructure, and API. Please include the necessary project structure, domain models, and any other relevant information.

**Why**: I wanted to get a feature spec for a basic repo structure done relatively quickly, to see output and give feedback to agent on basic project structure

**Review**: I took the prompt, changed my .NET version, clarified some naming conventions, and handed the prompt to Codex. It took my existing 3 projects and filled them in with necessary models, abstractions. All implementation is left TODO. 


**Prompt Used:** (Codex) Change the core project and all references to Core to be Domain. Clarify the purpose of DependencyInjection project, and clarify the separation of concerns between the three projects to me. In addition, clarify the entity mapping approach of the project.

**Why**: Needed name changes for domain (my preferred convention), as well as clarification on the purpose of a couple components.

**Review**: Kept name changes, and gained the following information about approach:

Domain: Business models, exceptions, abstractions, engines, and app services. No knwoledge about API, HTTP, or Yahoo provider
Infrastructure: Implements domain contracts and handles HTTP calls, DTOs, mapping, etc
API: Routing, response contracts, error responses. No knowledge about business logic or domain models

DependencyInjection registers services with DI containers (startup wiring)

Mapping strategy for Yahoo response: Yahoo JSON -> Yahoo DTO -> Domain models, happening through YahooResponseMapper

Mapping strategy for API response: Domain DailySummary -> DailySummaryResponse, happening through DailySummaryResponseMapper


**Prompt used:** Move the API project into the top-level `api` folder and update the solution and references.

**Why:** Keep the HTTP delivery project visibly separated from the Domain and Infrastructure projects.

**Review:** Moved the complete backend under `api/`: the solution, Domain and Infrastructure projects, API project, backend tests, and backend package/build configuration. The API project now also sits with the other source projects at `api/src/StockData.Api`. The repository root is now available for the future frontend. Updated relative project references and verified the backend solution builds cleanly with all placeholder tests skipped.

**Prompt used:**

**Why:**

**Review**

## 3. Vue frontend

**Prompt used:** 

**Why:** 

**Review:** 
