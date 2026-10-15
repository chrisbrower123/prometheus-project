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


**Prompt Used:** (Implementation spec guide attached) Ok, using everything we just discussed about planned architecture, make an implementation spec for a Codex agent that scaffolds our three .NET projects: Core, Infrastructure, and API. Please include the necessary project structure, domain models, and any other relevant information.

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


## Issue 2. Test 
**Prompt used:** It is time to begin drafting our tests, defining essential test cases for the API endpoints. We will begin with the domain model. Seems to me we will need to test each exception, model properties, the aggreagator, and the exposed summary service. Cover all edge cases. Draft a test list and I will review output and add missing cases.

**Why:** Starting implementation with domain test cases, first need to nail down expected behavior before implementation. First, we're writing tests

**Review** Claude gave me a very long list of tests, many of which were redundant or did not need to be tested (mostly in validation of domain models). I kept necessary domain validations, clarified behaviors of models, 


**Prompt used:** (Implementation spec guide attached) DECISION 1: service rejects default symbol.

DECISION 2: Model validates itself

DECISION 3: Should be immutable

DECISION 4: Reject

DECISION 5: It is unrealistic that this would happen

DECISIOn 6: Deduplicate the double-count

DECISION 7: REVIEW Timezone tests later

DECISION 8: Anomalous bars are rejected

DECISION 9: Window is anchored at now

DECISION 10: 404 here is fine


With these decisions, draft an implementation spec for codex to draft these tests.



**Why:** (Codex) Implementation spec for tests defined by my preferences and decisions, and I review Codex output later

**Review:** The spec looks good, handed off to copilot for test drafting



**Prompt used:** (Implementation spec) Draft the Domain/Core unit tests, writing real tests while not implementing domain logic. 

**Why:** Establish the expected domain behavior and edge cases before implementing the production logic.

**Review:** Wayyyy too many tests, next prompt will be trimming these to essential edge and success cases.


**Prompt used:** (Codex) Reduce the bulk of the domain test class focusing on validation, exception contracts, aggregator behavior, and service orchestration. 

**Why:** Needed to trim test cases, 150 tests as 1/3 of testing project is too bulky for this behavior

**Review:** Muchhhh better and focused test cases, keeping output, pushing to github, and moving to API tests



**Prompt used:** Using the same strategy, draft me tests for the Infrastructure project, let me make decisions, and we will draft an Infrastructure test spec.

**Why:** Needed basic test cases for the Infrastructure project to review and decide before drafting spec

**Review:** Way too many tests again, made 14 decisions for claude and will ask for implementation spec next

**Prompt Used**: Decision 1: Correct

D2: Ignore unexpected data

D3: Use first result

D4:Empty series, but this should be decided in a separate way

D5: Truncate

D6: Fallback

D7:Sounds good

D8: We can wrap this as ProvderUnavailable

D9:Cap the value

D10: Surface as retry-after

D11: 2 retries 

D12: Tolerate it

D13: Allowed hhtp, tests need it

D14: Use curl where possible. 

**Why:** Need to clarify important app behavior before drafting test spec.

**Review:** Output looks good. Handing off to codex for implementation



**Prompt used:**(Spec attached to codex) Implement according to spec

**Why:** Get a baseline of tests before implementation done by codex to a strict standard

**Review:** Output looks good.



**Prompt used:** Using the same strategy, draft me tests for the API project, let me make decisions, and we will draft an API test spec.

**Why:** Needed basic test cases for the API project to review and decide before drafting spec

**Review:** Way too many tests again, made 12 decisions for claude and will ask for implementation spec next


**Prompt used:**
D1: Fake provider, not service

D2: 429 here makes sense

D3: 504 split does make sense, but keep it simple to 502

D4: Status code pages are okay

D5: Make handler return false and don't log an error

D6: We do need to deal with CORS, so expect that

D7: Convention works

D8: 500 here

D9: Health does not call yahoo

D10: Only rule from section 2

D11: Trim trailing zeros

D12: 28 is good for method count.


Draft implementation spec

**Prompt used:**(Spec attached to codex) Implement according to spec

**Why:** Get a baseline of tests before implementation done by codex to a strict standard

**Review:** Output looks good.

## 3. API Implementation

**Prompt used:** Write me an implementation guide for codex to implement the domain (i renamed core to domain) tests. Tell it it cannot loosen tests and if it wants to do so it needs approval first. Otherwise, it should iterate until all tests are passing. 

**Why:** Needed a quick guide so codex does not go off the rails while implementing, sticks to structure, and greens tests

**Review:** Spec looks good. Handing to codex for domain project implementation



**Prompt used:** (Implementation guide provided to codex)

**Why**: Codex implements test-drivenly, and must request any changes to tests

**Review:** Codex found a contradictory test, and asked if it could alter. I approved, and suite is green.



**Prompt used:** Alter the guide for the infrastructure test suite

**Why:** I wanted a more focused guide for codex's next step

**Review:** Handing to codex for implementation



**Prompt used:** (Implementation guide provided to codex)

**Why**: Codex implements test-drivenly, and must request any changes to tests

**Review:** Codex found a contradictory test, and asked if it could alter. I approved, and suite is green.


**Prompt used:** Alter the guide for the api test suite

**Why:** I wanted a more focused guide for codex's next step

**Review:** Not necessary, handing to codex for implementation


**Prompt used:** (Implementation guide provided to codex)

**Why**: Codex implements test-drivenly, and must request any changes to tests

**Review:** Codex found 3 test changes. Eacch was a typo in the original tests, and I approved each.

## 4. Frontend Implementation

**Prompt Used:** Here is my backend endpoint and how to use it (pasted), along with project description. I am building a Vue 3 and Vite project for the frontend. Provide a suggested component architecture and base project layout, as well as suggested libraries for generating graphs with our data.

**Why:** I am beginning frontend implementation and want suggestions about architecture and tools to review before getting an implementation spec ready for codex.

**Review**: Decided to go with native chart.js library through vue. I agree with component structures, and it made sure to let me know to be careful of timezones



**Prompt Used:**: Ok. let's use chart.js via vue-chartjs and I agree with your component layout. Build me a spec for codex (spec guide included) to implement base file structures, no implementation

**Why:** I want a component spec guide for codex to set up frontend project structure

**Review**: Output looks good, handing to codex for implementation

**Prompt Used:**: (Scaffolding spec provided) to codex

**Why:** Codex implements frontend structure for me to verify

**Review:** Looks as planned, moving on to implementation.