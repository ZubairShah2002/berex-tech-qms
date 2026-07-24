**Berex Tech - QMS**

Enterprise Software Architecture Blueprint

**Version 1.1 (Frozen Edition)**

  -----------------------------------------------------------------------
  **Property**             **Value**
  ------------------------ ----------------------------------------------
  Document ID              BEREX-QMS-ARCH-v1.1-FROZEN

  Version                  1.1 (Frozen Edition)

  Classification           CONFIDENTIAL

  Date                     July 2026

  Author                   Berex Tech Architecture Division

  Status                   FROZEN --- Approved for Development

  Scope                    Complete System Architecture --- Chapters 1
                           through 30

  Freeze Authority         Architecture Review Board
  -----------------------------------------------------------------------

*This document is the single source of truth for the Berex Tech - QMS
engineering team.*

*No architectural changes shall be made after freeze except through
formal version upgrade.*

**Document Control**

**Version History**

  ---------------------------------------------------------------------------------
  **Version**   **Date**   **Author**     **Description**
  ------------- ---------- -------------- -----------------------------------------
  0.1           June 2026  Architecture   Initial draft --- Sections 1 through 6
                           Team           delivered for review

  0.2           June 2026  Architecture   Phases 4 through 8 added --- Core Domain
                           Team           Modules, Quality Inspection, NC/CAPA,
                                          Document Management, Audit Management

  0.3           July 2026  Architecture   Phases 9 through 13 added --- Training,
                           Team           AI/ML, Reporting, Notification,
                                          Integration

  0.4           July 2026  Architecture   Phases 14 through 19 added --- Logging,
                           Team           CI/CD, Security, DR, Scalability,
                                          Governance

  1.0           July 2026  Architecture   All 19 phases complete --- submitted for
                           Team           pre-freeze audit

  1.1           July 2026  Architecture   Post-audit revision: merged into single
                           Team           master document; resolved numbering
                                          conflicts; added Offline Architecture, AI
                                          Governance, expanded Supplier Quality,
                                          Calibration, and Training architectures;
                                          renamed project to Berex Tech - QMS
  ---------------------------------------------------------------------------------

**Change Log (v1.0 to v1.1)**

  ----------------------------------------------------------------------------------
  **Change   **Chapter**   **Change Description**          **Rationale**
  ID**                                                     
  ---------- ------------- ------------------------------- -------------------------
  CL-001     All           Renamed project from BT-AIQMS   Corporate branding
                           to Berex Tech - QMS throughout  alignment
                           entire document                 

  CL-002     All           Merged Phase 1--19 into single  Audit finding C.1 ---
                           master document with unified    resolve numbering
                           chapter numbering (Chapters     conflict between
                           1--30)                          chat-delivered and
                                                           docx-delivered content

  CL-003     All           Removed duplicate content from  Audit finding C.2 ---
                           parallel document generation    eliminate version
                           runs                            ambiguity

  CL-004     15            Expanded Supplier Quality from  Audit finding C.3 ---
                           business requirements to full   bounded context lacked
                           implementation architecture     implementation
                                                           specification

  CL-005     16            Expanded Calibration from       Audit finding C.3 ---
                           business requirements to full   bounded context lacked
                           implementation architecture     implementation
                                                           specification

  CL-006     17            Expanded Training and           Audit finding C.3 ---
                           Competency from business        bounded context lacked
                           requirements to full            implementation
                           implementation architecture     specification

  CL-007     20            Added complete Offline-First    Audit finding C.4 ---
                           Architecture chapter            shop-floor offline
                                                           capability was required
                                                           but unspecified

  CL-008     19            Added AI Governance and Safety  Audit finding C.5 --- AI
                           chapter                         operational guardrails
                                                           insufficient for
                                                           regulated manufacturing

  CL-009     Front matter  Added Document Control, Version Audit finding A ---
                           History, Change Log, and Freeze documentation structure
                           Status                          improvements
  ----------------------------------------------------------------------------------

**Freeze Status**

  -----------------------------------------------------------------------
  **Property**             **Value**
  ------------------------ ----------------------------------------------
  Document Status          FROZEN

  Freeze Date              July 2026

  Freeze Authority         Architecture Review Board

  Next Permitted Revision  Version 2.0 (post-implementation feedback
                           cycle)

  Change Control           Any modification requires formal Change
                           Request approved by Quality Manager and
                           Technical Lead

  Controlled Document ID   ARCH-0001 (registered in Berex Tech - QMS
                           Document Control upon system go-live)
  -----------------------------------------------------------------------

**Table of Contents**

Document Control

Chapter 1 --- Project Vision and Business Justification

Chapter 2 --- System Overview and Module Map

Chapter 3 --- Architecture Decisions and Technology Stack

Chapter 4 --- Module Specifications

Chapter 5 --- Database Architecture

Chapter 6 --- UX/UI Blueprint

Chapter 7 --- Domain Model and Bounded Contexts

Chapter 8 --- Shared Kernel and Cross-Cutting Infrastructure

Chapter 9 --- Workflow and Process Automation

Chapter 10 --- Quality Inspection Architecture

Chapter 11 --- Non-Conformance and CAPA Architecture

Chapter 12 --- Document Management Architecture

Chapter 13 --- Audit Management Architecture

Chapter 14 --- Statistical Process Control Architecture

Chapter 15 --- Supplier Quality Architecture

Chapter 16 --- Calibration and Metrology Architecture

Chapter 17 --- Training and Competency Architecture

Chapter 18 --- AI/ML Integration Architecture

Chapter 19 --- AI Governance and Safety

Chapter 20 --- Offline-First Architecture

Chapter 21 --- Reporting and Analytics Engine

Chapter 22 --- Notification and Communication System

Chapter 23 --- Integration and API Gateway

Chapter 24 --- Security Architecture

Chapter 25 --- Development Roadmap

Chapter 26 --- Testing Strategy

Chapter 27 --- Deployment and Infrastructure

Chapter 28 --- Monitoring and Logging

Chapter 29 --- Risk Analysis

Chapter 30 --- Reading Guide and Governance

**Chapter 1 --- Project Vision and Business Justification**

**1.1 Purpose and Problem Statement**

Berex Tech - QMS is an enterprise Quality Management System
purpose-built for discrete manufacturing environments. It replaces
fragmented paper-based and spreadsheet-driven quality processes with a
unified digital platform that makes data accuracy, traceability,
provable record integrity, and institutional knowledge preservation
byproducts of daily work rather than additional administrative burdens.

The system addresses six persistent quality management challenges:
inspection data trapped in paper forms and disconnected spreadsheets
with no real-time visibility; root cause analysis and corrective actions
managed through email chains and shared drives with no systematic
follow-through; supplier quality tracked manually with no automated
scorecarding or trend detection; audit preparation consuming weeks of
document retrieval effort; institutional knowledge lost when experienced
personnel depart; and an inability to prove that any given inspection
used a calibrated gauge, a qualified inspector, and the correct released
specification revision.

A quiet sixth problem underlies them all: record integrity is
unprovable. No one can demonstrate that a given inspection used a
calibrated gauge, a qualified inspector, and the correct released
specification revision. Berex Tech - QMS makes one system the mandatory
path for every quality transaction, enforcing integrity gates that
validate these prerequisites before permitting data entry.

**1.2 Business Justification**

Versus the status quo, the cost of poor quality (scrap, rework, sorting,
customer returns, expedited replacements, audit findings) is typically
three to eight percent of revenue in discrete manufacturing. Even a ten
to fifteen percent reduction in repeat defects typically pays for a
system within twelve to twenty-four months.

Versus commercial QMS solutions (ETQ, MasterControl, Intelex, QT9), the
strategic asset Berex is building is not forms and workflows --- it is a
structured, queryable corpus of Berex-specific quality knowledge (defect
history, successful corrective actions, product-specific inspection
know-how) that powers an AI assistant no vendor can replicate, plus
data-model ownership enabling future manufacturing-intelligence
expansion.

**1.3 Target Users and Stakeholders**

  ------------------------------------------------------------------------------
  **Stakeholder**          **Relationship to     **Success Criteria**
                           System**              
  ------------------------ --------------------- -------------------------------
  Quality Inspectors       Primary daily users;  Inspection entry faster than
  (IQC/IPQC/OQC)           data creators         paper; no duplicate recording

  Quality Engineers        Heavy users; RCA/CAPA Instant defect history;
                           owners                AI-accelerated analysis; fewer
                                                 status-chasing emails

  Quality Supervisors      Approvers; daily      Real-time line/area quality
                           monitors              view; workload visibility
                                                 across the team

  Quality Manager          System owner;         Trustworthy KPIs; audit-ready
                           decision maker        at any moment; declining
                                                 repeat-defect rate

  Supplier Quality         Supplier module       Automated scorecards;
  Engineers                owners                systematic SCAR tracking

  Internal Auditors        Audit module users    Checklists, findings, and
                                                 evidence in one place

  Production/Engineering   Defect reporters,     Simple reporting; clear tasks
                           CAPA action owners    assigned to them

  Top Management           Dashboard consumers   Monthly quality story in five
                                                 minutes, backed by drill-down

  Suppliers (future        External portal users Direct SCAR responses,
  portal)                                        scorecard visibility

  Customers and            Indirect              Faster, evidence-backed
  Certification Bodies     beneficiaries         responses; clean audits
  ------------------------------------------------------------------------------

**1.4 Business Value and Expected Outcomes**

Measurable targets to set at go-live (baseline established in first
ninety days, targets measured at twelve months):

  -------------------------------------------------------------------------
  **KPI**                  **Target**     **Baseline Comparison**
  ------------------------ -------------- ---------------------------------
  Repeat defect rate       −30%           Defects whose root cause matches
                                          a previously closed CAPA

  RCA cycle time           −40%           Current average
  (detection to verified                  detection-to-root-cause duration
  root cause)                             

  CAPA on-time closure     \> 85%         Typical paper-based baseline:
  rate                                    40--60%

  CAPA effectiveness       \> 95%         Near-zero in most manual systems
  verification completion                 

  Audit preparation effort −70%           Document and evidence retrieval
                                          time

  Inspection data          Real-time      Versus end-of-day or end-of-week
  availability                            batch entry

  First Pass Yield         Daily by       Versus monthly aggregate
  visibility               line/product   

  Provable record          100% of        Currently unprovable
  integrity                inspections    
  -------------------------------------------------------------------------

**1.5 Long-Term Platform Vision**

Berex Tech - QMS v1 is the quality data foundation with provable
integrity. The long-term company vision progresses through five
horizons: the quality data foundation (this system), production quality
integration (MES/ERP connectivity), maintenance-quality correlation, a
supplier collaboration portal, and enterprise manufacturing intelligence
powered by the accumulated quality knowledge corpus.

**Chapter 2 --- System Overview and Module Map**

**2.1 System Purpose**

Berex Tech - QMS is a single web-based (desktop plus tablet) enterprise
platform through which all quality inspection, defect, problem-solving,
supplier, audit, and document activities are executed, approved, traced,
analyzed, and learned from --- with provable record integrity and an AI
assistant grounded exclusively in Berex\'s own data.

**2.2 Module Inventory**

The system comprises twelve primary domain modules, each designed as a
self-contained bounded context following Domain-Driven Design
principles:

  --------------------------------------------------------------------------
  **Module**        **Domain Scope**    **Key Capabilities**
  ----------------- ------------------- ------------------------------------
  Quality           IQC, IPQC, OQC      Checklist execution, sampling plans,
  Inspection        inspection          measurements, integrity gates, lot
                    lifecycle           disposition

  Defect /          NCR lifecycle from  Multi-source capture,
  Non-Conformance   detection to        classification, containment,
                    closure             investigation, disposition

  CAPA Management   Corrective and      Root cause analysis, action
                    preventive action   planning, effectiveness verification
                    lifecycle           

  Document Control  Controlled document Version control, approval workflows,
                    lifecycle           distribution, acknowledgment
                                        tracking

  Audit Management  Internal and        Planning, scheduling, checklist
                    external audit      execution, findings, follow-up
                    lifecycle           

  Supplier Quality  Supplier evaluation Scorecards, SCAR workflow, approved
                    and incoming        supplier list, supplier portal
                    quality             

  Calibration and   Equipment           Equipment registry, scheduling,
  Metrology         calibration         certificates, gauge control, escaped
                    lifecycle           measurement analysis

  Training and      Personnel           Skill matrix, training plans,
  Competency        qualification       qualification validation, competency
                    lifecycle           assessment

  Product Catalog   Product and         Part definitions, revision control,
                    specification       specification parameters, BOM
                    management          references

  Statistical       SPC charts and      Control charts, Cp/Cpk, trend
  Process Control   process capability  detection, out-of-control response

  AI/ML Engine      Predictive          Defect prediction, anomaly
                    analytics and       detection, RCA suggestion, document
                    intelligent         classification
                    assistance          

  Identity and      Authentication,     JWT, RBAC, multi-tenancy,
  Access            authorization,      e-signatures, session management
                    tenant management   
  --------------------------------------------------------------------------

**2.3 Module Interdependency Rationale**

The modules interlock deliberately. A failed inspection auto-creates a
non-conformance record. A recurring non-conformance triggers CAPA
initiation. CAPA effectiveness verification may require a follow-up
inspection. Supplier scorecards aggregate incoming inspection results
and SCAR outcomes. The AI engine consumes data from all modules
(read-only) to power cross-domain analytics. This interconnection is by
design --- it ensures that quality data flows naturally through the
system without manual re-entry, and that every quality event creates a
traceable chain of records.

However, every module maintains strict data ownership. Cross-module
references use identifiers, not direct object references. Communication
occurs through published domain events, not shared database tables. This
ensures that each module can be developed, tested, and eventually
extracted into a standalone service without cascading changes across the
codebase.

**Chapter 3 --- Architecture Decisions and Technology Stack**

**3.1 Architecture Style: Modular Monolith**

Berex Tech - QMS adopts a modular monolith architecture --- a single
deployable unit composed of well-separated domain modules with enforced
boundaries. This choice provides the development velocity of a monolith
(single deployment pipeline, shared transaction context, simple
debugging) with the architectural clarity of microservices (module
isolation, independent development, documented extraction seams).

The documented extraction seams allow future decomposition into
microservices when scaling demands warrant it. Each module communicates
with others exclusively through a domain event bus or defined
anti-corruption layers, making extraction a bounded operation rather
than a full rewrite.

**Decision rationale:** A team of four to six developers cannot sustain
the operational overhead of twelve microservices (independent
deployments, distributed tracing, network partitioning, eventual
consistency everywhere). The modular monolith gives module independence
without operational complexity. When the team grows to fifteen-plus and
specific modules need independent scaling, extraction is straightforward
because the boundaries are already enforced.

**3.2 Internal Architecture: Clean Architecture with DDD**

Each module follows Clean Architecture principles with four layers:
Domain (entities, value objects, aggregates, domain services, domain
events --- no external dependencies), Application (use cases, commands,
queries, CQRS handlers, application services), Infrastructure
(persistence implementations, external service adapters, messaging), and
Presentation (REST API controllers, request/response DTOs, input
validation).

Dependencies point inward: Presentation depends on Application,
Application depends on Domain, Infrastructure implements Domain
interfaces. The Domain layer has zero dependencies on frameworks,
databases, or external services.

**3.3 Key Architecture Patterns**

  ------------------------------------------------------------------------
  **Pattern**        **Where Applied**  **Rationale**
  ------------------ ------------------ ----------------------------------
  Domain-Driven      All modules        Each module is a bounded context
  Design                                with its own ubiquitous language,
                                        aggregates, and domain events

  CQRS               Reporting, SPC, AI Separate read/write models where
                     Engine             query patterns differ
                                        significantly from command
                                        patterns

  Repository Pattern All persistence    Abstract data access behind
                                        domain-oriented interfaces;
                                        enables testing without database

  Event-Driven       Cross-module       Loose coupling; modules react to
  Communication      integration        events rather than calling each
                                        other directly

  Dependency         All layers         Constructor injection throughout;
  Injection                             composition root at application
                                        startup

  Specification      Complex queries    Encapsulate business rules as
  Pattern                               composable query specifications

  Unit of Work       Transaction        Coordinate multi-aggregate changes
                     management         within a single bounded context

  Outbox Pattern     Event publishing   Guarantee at-least-once event
                                        delivery using a transactional
                                        outbox table
  ------------------------------------------------------------------------

**3.4 Technology Stack**

  -------------------------------------------------------------------------------
  **Layer**          **Technology**         **Version/Notes**
  ------------------ ---------------------- -------------------------------------
  Runtime            .NET 8 (LTS)           Cross-platform; strong DDD/Clean
                                            Architecture ecosystem

  Primary Database   PostgreSQL 16          ACID compliance, JSONB support,
                                            partitioning, full-text search

  Cache / Session    Redis 7                Distributed caching, session storage,
                                            pub/sub for real-time notifications

  API                ASP.NET Core Web API   REST with OpenAPI/Swagger
                                            documentation

  Authentication     JWT (access + refresh  Stateless authentication with
                     tokens)                Redis-backed token revocation

  Authorization      Custom RBAC engine     Role-based with permission sets,
                                            tenant-scoped, resource-level where
                                            needed

  ORM                Entity Framework Core  Code-first migrations, LINQ queries,
                     8                      change tracking for audit

  Message Bus        MediatR (internal)     In-process mediator for commands,
                                            queries, and domain events

  Background Jobs    Hangfire               Scheduled jobs, retry policies,
                                            dashboard monitoring

  Search             PostgreSQL FTS +       Full-text search with trigram
                     pg_trgm                similarity; Elasticsearch reserved
                                            for future scale

  AI/ML              Python microservice    Model serving isolated from main
                     (FastAPI)              application; gRPC integration

  LLM Provider       Provider-abstracted    Abstraction layer enables provider
                     (OpenAI/Azure/local)   switching; on-premise fallback
                                            documented

  Containerization   Docker + Docker        Development and production parity;
                     Compose                single-command environment setup

  CI/CD              GitHub Actions         Trunk-based development, automated
                                            testing, one-click production deploy

  Monitoring         Prometheus + Grafana   Metrics collection, dashboards,
                                            alerting

  Logging            Serilog + Seq (or ELK) Structured logging with correlation
                                            IDs, centralized aggregation

  Object Storage     MinIO (S3-compatible)  Document attachments, calibration
                                            certificates, training materials

  Frontend           React 18 + TypeScript  Component library, responsive design,
                                            tablet-first for floor screens
  -------------------------------------------------------------------------------

**3.5 Master Data Management Governance**

Master data entities (parts, suppliers, equipment, personnel, defect
catalogs) are governed by named owners with approval workflows. Each
master data domain has a designated steward responsible for data
quality. Import validators enforce referential integrity and business
rules at ingestion. A go-live gate requires master data quality
verification before production deployment. The MDM governance model
ensures that the data foundation supporting quality decisions is
trustworthy from day one.

**Chapter 4 --- Module Specifications**

This chapter provides the business-level specification for each domain
module. Implementation architecture for modules that require expanded
detail (Supplier Quality, Calibration, Training) is provided in
dedicated chapters later in this document.

**4.1 Quality Inspection Module**

The Quality Inspection module is the primary data entry surface of Berex
Tech - QMS and the highest-risk component from a user adoption
perspective. It supports three inspection types --- Incoming Quality
Control (IQC), In-Process Quality Control (IPQC), and Outgoing Quality
Control (OQC) --- each with configurable checklists, sampling plans, and
approval workflows.

**4.1.1 Key Features**

**Integrity Gates:** Before an inspector can begin data entry, the
system silently validates three prerequisites: the inspector holds a
current qualification for the inspection type and product family; the
selected measurement equipment has a valid calibration certificate that
has not expired; and the checklist references the currently released
specification revision. If any gate fails, the system blocks entry and
displays a clear remediation path. When all gates pass, they are
invisible --- zero friction for compliant inspections.

**Sampling Plans:** Configurable sampling plans based on AQL tables
(ANSI/ASQ Z1.4 for attributes, Z1.9 for variables). The system supports
normal, tightened, and reduced inspection levels with automatic
switching rules based on lot history. Skip-lot qualification is tracked
per part-supplier combination.

**Measurement Recording:** Each measurement persists with unit,
specification snapshot, gauge foreign key, operator, machine, and
time-sequence --- making the data SPC-ready and MSA-ready from the
moment of capture.

**Disposition Workflow:** Auto-computed lot result from sampling rules.
Fail triggers lot quarantine. Disposition options include Accept,
Accept-with-Deviation (creates a Deviation record, requires Manager
e-signature), Sort, Rework, Return-to-supplier, and Scrap --- each with
required fields, role gates, and signature capture.

**Traceability:** Full history by part, lot, supplier, line, inspector,
and gauge. Each result links the checklist version, specification
snapshot, gauge calibration state, and inspector qualification in force
at the time of inspection.

**4.2 Defect Management (Non-Conformance) Module**

One structured lifecycle for every nonconformity, from any source, until
closure --- the system\'s central case file. Converts scattered problem
reports into classified, owned, traceable cases that feed Pareto
analysis, supplier scorecards, RCA, and CAPA. Repeat-defect detection
lives here. Traceability and genealogy make containment and recall
possible.

**Multi-Source Capture:** Automatic creation from failed inspections;
manual creation from line finds, customer complaints, audit findings,
and supplier notifications.

**Classification:** Hierarchical defect catalog (category to type),
severity (Critical/Major/Minor with embedded definitions), detection
point, suspected origin (supplier/process step/design/handling),
quantity affected, and containment status.

**Traceability Block:** Part plus revision, lot and/or serial (per the
part\'s serialization mode), supplier plus supplier lot, line/station,
work order, customer (if outgoing), source inspection/complaint/audit
reference. Links into the lot genealogy structure so \'what else is
affected and where did it ship\' is answerable.

**4.3 CAPA Management Module**

Manages the corrective and preventive action lifecycle from initiation
through root cause analysis, action planning, implementation, and
effectiveness verification. The distinguishing design decision:
effectiveness verification is structurally mandatory. A CAPA physically
cannot reach Closed-Effective status without a scheduled, evidenced
verification step --- because that is the step every manual QMS skips
and every auditor checks.

**Root Cause Analysis:** Supports structured methodologies (5-Why,
Fishbone/Ishikawa, Fault Tree Analysis) with guided templates. The AI
engine provides similar-case retrieval and root-cause suggestions once
sufficient historical data exists (staged to development Phase 4).

**Action Tracking:** Each action has an owner, a due date, an evidence
requirement, and a completion workflow. Overdue actions trigger
escalation notifications.

**Effectiveness Verification:** Scheduled at CAPA planning time.
Verification criteria defined upfront. Evidence collected at
verification time. Only after verified effective can the CAPA be closed.
If verification fails, the CAPA is reopened with a new action cycle.

**4.4 Document Control Module**

Manages the lifecycle of controlled documents --- quality procedures,
work instructions, specifications, forms, and templates. Enforces
version control, approval workflows, and distribution tracking with
document acknowledgment.

**Version Control:** Every document revision creates a new version with
full diff tracking. Only one version can be in Released status at any
time. Superseded versions are archived and remain accessible for
historical reference.

**Approval Workflow:** Configurable per document type. Typical flow:
Draft, Under Review, Pending Approval, Released. Approval requires
e-signature. Emergency release path available with mandatory
post-release review.

**Distribution and Acknowledgment:** When a document is released, the
system identifies affected personnel based on role, department, and
competency requirements. Acknowledgment tracking ensures every required
reader has confirmed receipt and understanding.

**4.5 Audit Management Module**

Supports internal quality audits, supplier audits, and preparation for
external certification audits. Manages the complete audit lifecycle from
annual planning through execution, findings, corrective actions, and
follow-up.

**Audit Planning:** Annual audit schedule with risk-based
prioritization. Process/area coverage matrix ensures complete system
coverage over the audit cycle.

**Execution:** Configurable checklists per audit type and standard (ISO
9001, IATF 16949, AS9100). Evidence attachment during audit. Finding
classification (Major NC, Minor NC, Observation, Opportunity for
Improvement).

**Finding to CAPA Linkage:** Audit findings classified as Major or Minor
NC automatically trigger CAPA initiation with the audit finding as the
source reference.

**4.6 Product Catalog Module**

Manages product definitions, part numbers, revisions, specification
parameters, and BOM references. Serves as the master data source that
Quality Inspection, Non-Conformance, and Supplier Quality modules
reference for product-specific quality requirements. Part revision
changes trigger downstream checklist review notifications.

**4.7 Statistical Process Control Module**

Provides real-time statistical process control charts (X-bar/R, X-bar/S,
Individual/Moving Range, p, np, c, u), process capability analysis (Cp,
Cpk, Pp, Ppk), and automated out-of-control detection using Western
Electric rules and Nelson rules. SPC consumes measurement data from the
Quality Inspection module through domain events. Out-of-control
conditions trigger configurable responses including operator alerts,
supervisor notifications, and automatic inspection tightening.

**4.8 AI/ML Engine Module**

The AI/ML Engine is an assistive intelligence layer that enriches the
user experience without blocking critical quality processes. All AI
capabilities are exposed as asynchronous services. The engine maintains
its own denormalized analytical data store populated through
event-driven data ingestion from upstream modules. Detailed architecture
is provided in Chapter 18. Governance and safety controls are specified
in Chapter 19.

**Chapter 5 --- Database Architecture**

**5.1 Data Model Philosophy**

The Berex Tech - QMS database is designed around six principles: every
quality record is immutable after approval (soft-delete with audit
trail, never physical deletion of quality data); every table includes
tenant_id for multi-tenancy isolation; all timestamps are stored in UTC
with timezone; every mutation is captured in a centralized audit_log
table; foreign keys enforce referential integrity at the database level;
and master data entities use surrogate keys with natural key uniqueness
constraints.

The schema follows a module-per-schema approach in PostgreSQL: each
bounded context owns its schema (e.g., inspection.\*, ncr.\*, capa.\*,
document.\*, audit.\*, supplier.\*, calibration.\*, training.\*, spc.\*,
ai_engine.\*, catalog.\*, identity.\*). Cross-schema references use
foreign keys to the referenced module\'s primary identifiers only ---
never to internal implementation tables.

**5.2 Core Entity Model**

  --------------------------------------------------------------------------------------
  **Entity**          **Schema**    **Purpose**             **Key Relationships**
  ------------------- ------------- ----------------------- ----------------------------
  inspection_record   inspection    Individual inspection   checklist_version, lot,
                                    execution               inspector, sampling_plan

  measurement         inspection    Individual measurement  inspection_record,
                                    data point              equipment, spec_snapshot

  non_conformance     ncr           NCR case file           source_inspection, lot,
                                                            part_revision, supplier

  capa_record         capa          Corrective/preventive   source_nc,
                                    action                  source_audit_finding,
                                                            effectiveness_verification

  document_version    document      Controlled document     document_master, approver,
                                    revision                distribution_list

  audit_record        audit         Audit execution         audit_plan, auditor,
                                                            checklist, findings

  supplier_record     supplier      Supplier master data    scorecard, scar_records,
                                                            approved_parts

  equipment           calibration   Measurement equipment   calibration_records,
                                    registry                certificates

  training_record     training      Training completion     employee, course,
                                    record                  qualification, competency

  part_revision       catalog       Part/product version    part_master, specifications,
                                                            checklist_mappings

  spc_control_chart   spc           Control chart           part_characteristic,
                                    definition              control_limits, data_series

  tenant              identity      Tenant (site/facility)  users, roles, configuration
  --------------------------------------------------------------------------------------

**5.3 Audit Trail Design**

Every state-changing operation across all modules writes a record to the
centralized audit_log table. The audit log captures: tenant_id, user_id,
timestamp (UTC), entity_type, entity_id, action (CREATE, UPDATE, DELETE,
STATE_CHANGE, APPROVE, SIGN), old_value (JSONB snapshot of changed
fields), new_value (JSONB snapshot), source_ip, correlation_id, and
module_name. The audit log table is append-only with no UPDATE or DELETE
permissions granted to the application role. A separate read-only
reporting role accesses audit data for compliance queries.

**5.4 Multi-Tenancy Data Isolation**

Every business table includes a tenant_id column. Row-Level Security
(RLS) policies in PostgreSQL enforce tenant isolation at the database
level --- even if the application layer has a bug, one tenant cannot
access another\'s data. The RLS policy reads the tenant context from a
session variable set at connection time. This provides defense-in-depth
beyond application-layer filtering.

**5.5 Data Retention and Archival**

  -------------------------------------------------------------------------
  **Data Tier**  **Retention**   **Storage**              **Access
                                                          Pattern**
  -------------- --------------- ------------------------ -----------------
  Hot (Active)   Current year +  Primary PostgreSQL with  Real-time
                 1 year          full indexing            dashboards,
                                                          operational
                                                          queries

  Warm           2--5 years      PostgreSQL partitioned   Trend analysis,
  (Historical)                   tables, reduced indexing periodic reports

  Cold (Archive) 5--15 years     Compressed export to     Regulatory audit,
                 (regulatory     object storage (Parquet) legal discovery
                 dependent)                               

  Purged         Beyond          Deleted per data         Not accessible
                 retention       governance policy        
                 period                                   
  -------------------------------------------------------------------------

**Chapter 6 --- UX/UI Blueprint**

**6.1 Design Philosophy: Two-Density Model**

Berex Tech - QMS operates in two distinct usage contexts that demand
fundamentally different interface designs. Floor Mode serves quality
inspectors on tablets in production environments --- large touch
targets, minimal navigation, maximum data entry speed, high contrast for
factory lighting. Office Mode serves quality engineers, managers, and
auditors on desktop screens --- dense information display, multi-panel
layouts, advanced filtering, comprehensive dashboards.

The Floor Mode tablet inspection screen is the highest-risk design
surface in the system. If inspectors find it slower than paper, adoption
fails regardless of how sophisticated the backend architecture is. This
screen receives more design iteration budget than any other component.

**6.2 Six Non-Negotiable UX Rules**

**Rule 1 --- Zero-State Guidance:** Every screen explains what to do
when empty. No blank screens with no context.

**Rule 2 --- Integrity Gates Are Invisible When Satisfied:** Calibration
verified, qualification confirmed, spec revision current --- the
inspector never sees these checks unless one fails. Zero friction for
compliant work.

**Rule 3 --- Every Action Has an Undo Window:** Thirty-second undo for
data entry. Draft autosave every ten seconds. No lost work.

**Rule 4 --- Progressive Disclosure:** Show the minimum needed for the
current task. Advanced options expand on demand. Floor Mode hides
everything Office Mode shows.

**Rule 5 --- Consistent Confirmation:** Destructive actions require
explicit confirmation. Success feedback is immediate and visible. Error
messages include remediation paths.

**Rule 6 --- Keyboard-First for Office, Touch-First for Floor:** Office
Mode supports full keyboard navigation and shortcuts. Floor Mode
supports gesture-based navigation and large touch targets.

**6.3 Key Screen Inventory**

  -----------------------------------------------------------------------------
  **Screen**      **Mode**   **Primary Action**    **Design Priority**
  --------------- ---------- --------------------- ----------------------------
  Inspection      Floor      Record measurements   Speed of data entry --- must
  Entry                      against checklist     beat paper

  Inspection      Office     Monitor daily         At-a-glance status across
  Dashboard                  inspection status     lines/products

  NCR Detail      Office     Manage NC lifecycle   Complete case file
                                                   visibility

  CAPA Workflow   Office     Drive corrective      Clear next-action visibility
                             actions               

  Quality         Office     Monitor KPIs          Trend clarity, drill-down
  Dashboard                                        capability

  Document        Office     Find and manage       Fast search, clear version
  Library                    documents             status

  Audit Execution Both       Record audit findings Checklist completion
                                                   efficiency

  Supplier        Office     Review supplier       Comparative ranking, trend
  Scorecard                  performance           visibility

  SPC Chart       Office     Monitor process       Real-time chart updates,
  Viewer                     control               rule violations

  Admin Console   Office     System configuration  Clear settings organization
  -----------------------------------------------------------------------------

**Chapter 7 --- Domain Model and Bounded Contexts**

**7.1 Bounded Context Map**

The bounded context map defines the relationships and integration
patterns between all domain modules within Berex Tech - QMS. Each module
maintains strict ownership of its data and exposes capabilities through
well-defined contracts.

  ----------------------------------------------------------------------------
  **Bounded         **Responsibility**   **Upstream       **Integration
  Context**                              Dependencies**   Pattern**
  ----------------- -------------------- ---------------- --------------------
  Identity and      User management,     None (root       Shared Kernel
  Access            authentication,      context)         
                    authorization,                        
                    tenant management                     

  Quality           Inspection planning, Identity,        Customer-Supplier
  Inspection        execution, sampling, Product Catalog  
                    result recording                      

  Non-Conformance   NC identification,   Inspection,      Customer-Supplier
                    classification,      Identity         
                    containment,                          
                    disposition                           

  CAPA Management   Corrective and       NC, Audit        Customer-Supplier
                    preventive action    Management       
                    lifecycle                             

  Document Control  Document lifecycle,  Identity         Conformist
                    version control,                      
                    approval workflows                    

  Audit Management  Audit planning,      Identity,        Customer-Supplier
                    execution, findings, Document Control 
                    follow-up                             

  Training and      Training records,    Identity,        Customer-Supplier
  Competency        competency           Document Control 
                    assessment, skill                     
                    matrix                                

  Product Catalog   Product definitions, Identity         Published Language
                    specifications,                       
                    quality parameters                    

  Supplier Quality  Supplier evaluation, Inspection,      Customer-Supplier
                    incoming inspection, Product Catalog  
                    scorecards                            

  Calibration       Equipment            Identity         Separate Ways
                    calibration                           
                    schedules, records,                   
                    certificates                          

  Statistical       SPC charts,          Inspection, AI   Customer-Supplier
  Process Control   capability analysis, Engine           
                    trend detection                       

  AI/ML Engine      Predictive           All contexts     Open Host Service
                    analytics, anomaly   (read-only)      
                    detection, NLP                        
                    processing                            
  ----------------------------------------------------------------------------

***Diagram: Domain Event Flow Architecture***

graph LR A\[Quality Inspection\] \--\>\|InspectionCompleted\| EB\[Event
Bus\] B\[Non-Conformance\] \--\>\|NonConformanceRaised\| EB C\[CAPA
Management\] \--\>\|CAPAInitiated\| EB D\[Document Control\]
\--\>\|DocumentApproved\| EB E\[Audit Management\]
\--\>\|AuditFindingRecorded\| EB EB \--\> F\[AI/ML Engine\] EB \--\>
G\[Notification System\] EB \--\> H\[SPC Module\] EB \--\> I\[Reporting
Engine\]

**7.2 Aggregate Design Principles**

**Rule 1 --- Single Transaction Boundary:** Each aggregate is modified
within a single database transaction. No transaction spans multiple
aggregates.

**Rule 2 --- Reference by Identity:** Cross-aggregate references use
identifiers, not direct object references. This enables independent
persistence and scaling.

**Rule 3 --- Minimal Aggregate Size:** Aggregates are as small as
possible while maintaining transactional consistency invariants.

**Rule 4 --- Event Publication:** Every state-changing operation on an
aggregate publishes at least one domain event, enabling downstream
modules to react asynchronously.

**7.3 Domain Event Catalog**

  -----------------------------------------------------------------------------------
  **Event Name**          **Source          **Payload Summary**   **Key Consumers**
                          Context**                               
  ----------------------- ----------------- --------------------- -------------------
  InspectionCompleted     Quality           Inspection ID,        SPC, NC, AI Engine
                          Inspection        result, product ID,   
                                            timestamps            

  NonConformanceRaised    Non-Conformance   NC ID, severity,      CAPA, Supplier
                                            product ID, defect    Quality, AI Engine
                                            type                  

  CAPAInitiated           CAPA Management   CAPA ID, source       Notification,
                                            NC/Audit, assigned    Training
                                            owner                 

  DocumentApproved        Document Control  Document ID, version, Training, Audit
                                            approver, effective   
                                            date                  

  AuditFindingRecorded    Audit Management  Finding ID, audit ID, CAPA, NC, AI Engine
                                            severity, area        

  CalibrationDue          Calibration       Equipment ID, due     Notification,
                                            date, last            Inspection
                                            calibration date      

  TrainingCompleted       Training          User ID, course ID,   Identity,
                                            competency level,     Inspection
                                            expiry                

  SupplierScoreUpdated    Supplier Quality  Supplier ID, new      AI Engine,
                                            score, evaluation     Notification
                                            period                

  EquipmentCalibrated     Calibration       Equipment ID, result, Inspection,
                                            certificate ID, next  Notification
                                            due                   

  QualificationExpiring   Training          User ID,              Notification,
                                            qualification ID,     Inspection
                                            expiry date           
  -----------------------------------------------------------------------------------

**Chapter 8 --- Shared Kernel and Cross-Cutting Infrastructure**

**8.1 Shared Value Objects**

Value objects in the shared kernel represent domain-agnostic concepts
that appear consistently across multiple bounded contexts. These objects
are immutable, equality-compared by value, and self-validating upon
construction.

  --------------------------------------------------------------------------
  **Value         **Fields**               **Used By**
  Object**                                 
  --------------- ------------------------ ---------------------------------
  TenantId        GUID tenant identifier   All modules

  UserId          GUID user identifier     All modules

  DateRange       Start (UTC), End (UTC)   Audit, CAPA, Calibration,
                                           Training

  Money           Amount (decimal),        Cost of Quality, Supplier
                  Currency (ISO 4217)      

  Attachment      File reference, MIME     NC, CAPA, Audit, Document,
                  type, size, hash         Calibration

  AuditMetadata   Created by, created at,  All entities
                  modified by, modified at 

  EmailAddress    Validated email string   Identity, Notification

  PersonName      First name, last name,   Identity, Training
                  display name             
  --------------------------------------------------------------------------

**8.2 Cross-Cutting Services**

  -----------------------------------------------------------------------------
  **Service**      **Responsibility**           **Implementation**
  ---------------- ---------------------------- -------------------------------
  Audit Logger     Immutable record of every    Database interceptor + outbox
                   state change                 pattern

  Event Bus        In-process domain event      MediatR notification handlers
                   dispatch                     

  Authorization    Permission evaluation        Custom RBAC engine with
  Service                                       permission cache

  Notification     Multi-channel notification   Template engine + channel
  Dispatcher       delivery                     adapters (email, in-app, SMS)

  File Storage     Secure file                  MinIO adapter with virus
  Service          upload/download/versioning   scanning

  Clock Service    Consistent UTC time source   Abstracted for testability

  Tenant Context   Current tenant resolution    Middleware sets tenant from
                                                JWT; RLS enforced at DB

  Correlation ID   Request tracing across       HTTP middleware
  Provider         layers                       generates/propagates
                                                correlation IDs
  -----------------------------------------------------------------------------

**8.3 Infrastructure Conventions**

All modules follow identical infrastructure conventions: database
migrations use Entity Framework Core with version-numbered migration
files; configuration follows the Options pattern with strongly-typed
settings classes; health checks are registered per module and aggregated
at the /health endpoint; structured logging uses Serilog with tenant_id,
user_id, and correlation_id enrichment on every log entry; and exception
handling follows a global filter that maps domain exceptions to
appropriate HTTP status codes without exposing internal details.

**Chapter 9 --- Workflow and Process Automation**

**9.1 Workflow Engine Architecture**

Berex Tech - QMS implements workflows as state machines within each
domain module rather than using an external workflow engine. This
decision keeps workflow logic co-located with business rules, ensures
compile-time type safety on state transitions, and avoids the impedance
mismatch of mapping domain concepts to a generic workflow engine\'s
abstractions.

Each state machine defines: valid states (as an enumeration), permitted
transitions (from-state to to-state with guard conditions), transition
actions (business logic executed during transition), authorization
requirements (which roles can trigger which transitions), event
publications (domain events emitted on transition), and time-based rules
(escalation timers, SLA warnings, auto-transitions).

**9.2 Core Workflows**

  --------------------------------------------------------------------------------------------
  **Workflow**   **States**             **Average      **Key Business Rules**
                                        Complexity**   
  -------------- ---------------------- -------------- ---------------------------------------
  Inspection     Planned, InProgress,   Medium         Integrity gates at start;
  Lifecycle      PendingReview,                        auto-disposition from sampling;
                 Approved, Rejected                    supervisor approval on fail

  NCR Lifecycle  Open,                  High           Severity-based escalation; mandatory
                 UnderInvestigation,                   containment for Critical; CAPA linkage
                 PendingDisposition,                   for Major/Critical
                 Closed, Reopened                      

  CAPA Lifecycle Initiated,             High           Mandatory effectiveness verification;
                 RCAInProgress,                        re-open on failed verification;
                 ActionPlanning,                       time-based escalation
                 Implementation,                       
                 PendingVerification,                  
                 ClosedEffective,                      
                 ClosedIneffective                     

  Document       Draft, UnderReview,    Medium         Single active version; e-signature on
  Lifecycle      PendingApproval,                      approval; distribution triggers
                 Released, Superseded,                 acknowledgment
                 Obsolete                              

  Audit          Planned, Scheduled,    Medium         Finding-to-CAPA linkage; coverage
  Lifecycle      InProgress,                           matrix tracking
                 FindingsRecorded,                     
                 ReportIssued,                         
                 FollowUpComplete                      

  SCAR Lifecycle Issued,                Medium         Response deadline tracking; scorecard
                 SupplierResponse,                     impact; repeat SCAR detection
                 UnderReview, Accepted,                
                 Rejected, FollowUp,                   
                 Closed                                

  Calibration    Scheduled, InProgress, Low-Medium     Due date enforcement; failed
  Lifecycle      Completed, Failed,                    calibration impact analysis;
                 Overdue                               certificate management

  Training       Assigned, InProgress,  Low-Medium     Competency validation; re-training
  Lifecycle      PendingAssessment,                    triggers; expiry monitoring
                 Completed, Expired                    

  Change Request Submitted,             Medium         Impact assessment required;
                 UnderReview, Approved,                cross-functional approval;
                 Implementing,                         effectiveness check
                 Verified, Closed                      

  Deviation      Requested,             Low            Time-limited approval; mandatory
  Request        UnderReview, Approved,                expiry; Manager e-signature
                 Expired, Closed                       
  --------------------------------------------------------------------------------------------

**9.3 Escalation Framework**

Each workflow implements a configurable escalation framework. Escalation
rules define: the condition (record in a specific state beyond a time
threshold), the escalation action (notification to next-level
authority), the escalation chain (up to three levels before automatic
flagging to Quality Manager), and the notification template. Escalation
timers are tenant-configurable per workflow and severity level. The
Hangfire scheduler evaluates escalation conditions every fifteen
minutes.

**9.4 E-Signature Integration**

Workflow transitions that constitute quality decisions (inspection
approval, NC disposition, CAPA closure, document release) require
electronic signatures. The e-signature captures: user identity
(authenticated via JWT), timestamp (UTC from server clock, not client),
action description, and the hash of the record state at signature time.
The signature implementation is designed to be hardenable to 21 CFR Part
11 requirements if regulatory scope demands it --- the data model
already captures all required elements.

**Chapter 10 --- Quality Inspection Architecture**

**10.1 Inspection Domain Model**

  ------------------------------------------------------------------------------
  **Entity**             **Type**       **Description**
  ---------------------- -------------- ----------------------------------------
  InspectionRecord       Aggregate Root Manages inspection lifecycle, integrity
                                        gate results, overall disposition

  InspectionChecklist    Entity (child) Versioned checklist bound to this
                                        inspection, snapshot of spec revision in
                                        effect

  ChecklistItem          Entity (child) Individual inspection characteristic
                                        with specification limits and
                                        measurement type

  Measurement            Value Object   Recorded value with unit, equipment
                                        reference, operator, timestamp

  IntegrityGateResult    Value Object   Pass/fail result for each gate
                                        (qualification, calibration, spec
                                        revision)

  SamplingPlan           Entity         AQL-based sampling configuration per
                         (referenced)   part-supplier-inspection-type
                                        combination

  LotDisposition         Value Object   Final lot decision with justification,
                                        approver signature, and timestamp

  InspectionAttachment   Value Object   Photos, measurement reports, or
                                        supporting evidence
  ------------------------------------------------------------------------------

**10.2 Integrity Gate Validation Sequence**

***Diagram: Integrity Gate Validation Flow***

sequenceDiagram participant Inspector participant System participant
CalibrationDB participant TrainingDB participant DocumentDB
Inspector-\>\>System: Start Inspection (lot, equipment, inspector)
System-\>\>CalibrationDB: Validate equipment calibration status
CalibrationDB\--\>\>System: Calibration valid until \[date\]
System-\>\>TrainingDB: Validate inspector qualification
TrainingDB\--\>\>System: Qualification valid for \[product family\]
System-\>\>DocumentDB: Validate checklist spec revision
DocumentDB\--\>\>System: Current released revision \[rev\]
System\--\>\>Inspector: All gates passed --- proceed with inspection

**10.3 Inspection API Surface**

  ---------------------------------------------------------------------------------------------------
  **Endpoint**                            **Method**     **Description**          **Authorization**
  --------------------------------------- -------------- ------------------------ -------------------
  /api/v1/inspections                     POST           Create new inspection    Inspector,
                                                         (triggers integrity      Engineer,
                                                         gates)                   Supervisor

  /api/v1/inspections/{id}/measurements   POST           Record measurement batch Inspector

  /api/v1/inspections/{id}/complete       PUT            Complete inspection,     Inspector
                                                         compute result           

  /api/v1/inspections/{id}/approve        PUT            Approve inspection       Supervisor
                                                         result (e-signature)     

  /api/v1/inspections/{id}/disposition    PUT            Set lot disposition for  Supervisor, Manager
                                                         failed inspection        

  /api/v1/inspections                     GET            List inspections with    All quality roles
                                                         filtering and pagination 

  /api/v1/inspections/{id}                GET            Get inspection detail    All quality roles
                                                         with all measurements    

  /api/v1/sampling-plans                  GET/POST/PUT   Manage sampling plan     Engineer, Manager
                                                         configurations           
  ---------------------------------------------------------------------------------------------------

**Chapter 11 --- Non-Conformance and CAPA Architecture**

**11.1 Non-Conformance Aggregate**

  --------------------------------------------------------------------------
  **Entity/Value      **Type**      **Description**
  Object**                          
  ------------------- ------------- ----------------------------------------
  NonConformance      Aggregate     Central entity managing NC lifecycle,
                      Root          severity, and disposition

  NCClassification    Value Object  Category, defect type, defect code from
                                    hierarchical catalog

  ContainmentAction   Entity        Immediate containment steps to isolate
                      (child)       nonconforming product

  Investigation       Entity        Root cause investigation with
                      (child)       methodology and findings

  DispositionRecord   Value Object  Final disposition (use-as-is, rework,
                                    scrap, return) with justification

  NCAttachment        Value Object  Photos, test reports, deviation requests

  ImpactAssessment    Value Object  Affected lots, shipped product
                                    evaluation, customer impact

  NCStatus            Enumeration   Open, UnderInvestigation,
                                    PendingDisposition, Closed, Reopened
  --------------------------------------------------------------------------

**11.2 NC Lifecycle State Machine**

***Diagram: Non-Conformance Lifecycle***

stateDiagram-v2 \[\*\] \--\> Open : Identified Open \--\>
UnderInvestigation : AssignInvestigator UnderInvestigation \--\>
PendingDisposition : InvestigationComplete PendingDisposition \--\>
Closed : DispositionApproved PendingDisposition \--\> UnderInvestigation
: RequiresMoreInfo Closed \--\> Reopened : NewEvidenceFound Reopened
\--\> UnderInvestigation : ReassignInvestigator Open \--\> Closed :
DuplicateOrInvalid

**11.3 NC Business Rules**

**Severity Escalation:** Critical and Major NCs automatically trigger
notification to quality management and plant leadership. Critical NCs
impose a mandatory containment action before investigation can proceed.

**Time-Based Escalation:** Configurable escalation timers alert
management when NC records remain in a state beyond defined thresholds.
Escalation rules are tenant-configurable.

**Mandatory CAPA Linkage:** NCs classified as Critical or Major, or
those with recurring defect patterns, require a linked CAPA record
before closure. The system validates this linkage during the closure
transition.

**Repeat Defect Detection:** When a new NC is created, the system
automatically searches for similar NCs by part, defect type, and
supplier within a configurable lookback window. Matches are flagged to
the investigator and may auto-trigger CAPA if threshold is exceeded.

**11.4 CAPA Aggregate**

  ----------------------------------------------------------------------------------
  **Entity/Value Object**     **Type**      **Description**
  --------------------------- ------------- ----------------------------------------
  CAPARecord                  Aggregate     Manages the full CAPA lifecycle from
                              Root          initiation to effectiveness verification

  RootCauseAnalysis           Entity        Structured RCA using 5-Why, Fishbone, or
                              (child)       FTA methodology with findings

  CorrectiveAction            Entity        Individual corrective action with owner,
                              (child)       due date, evidence requirement

  PreventiveAction            Entity        Preventive action to address systemic
                              (child)       causes

  EffectivenessVerification   Entity        Scheduled verification with criteria,
                              (child)       evidence, and result

  CAPASource                  Value Object  Reference to originating NC, audit
                                            finding, or customer complaint

  CAPAStatus                  Enumeration   Initiated, RCAInProgress,
                                            ActionPlanning, Implementation,
                                            PendingVerification, ClosedEffective,
                                            ClosedIneffective
  ----------------------------------------------------------------------------------

**11.5 CAPA Lifecycle State Machine**

***Diagram: CAPA Lifecycle***

stateDiagram-v2 \[\*\] \--\> Initiated : CAPACreated Initiated \--\>
RCAInProgress : StartRCA RCAInProgress \--\> ActionPlanning :
RCAComplete ActionPlanning \--\> Implementation : ActionsAssigned
Implementation \--\> PendingVerification : AllActionsComplete
PendingVerification \--\> ClosedEffective : VerificationPassed
PendingVerification \--\> RCAInProgress : VerificationFailed
ClosedEffective \--\> \[\*\] RCAInProgress \--\> ActionPlanning :
RCARevised

**11.6 NC/CAPA API Surface**

  -------------------------------------------------------------------------------------------------
  **Endpoint**                                **Method**   **Description**
  ------------------------------------------- ------------ ----------------------------------------
  /api/v1/non-conformances                    POST         Create NC (manual or from failed
                                                           inspection event)

  /api/v1/non-conformances/{id}/investigate   PUT          Submit investigation findings

  /api/v1/non-conformances/{id}/disposition   PUT          Record disposition decision
                                                           (e-signature)

  /api/v1/non-conformances/{id}/similar       GET          Find similar NCs for repeat detection

  /api/v1/capas                               POST         Initiate CAPA (from NC, audit finding,
                                                           or standalone)

  /api/v1/capas/{id}/rca                      PUT          Submit root cause analysis

  /api/v1/capas/{id}/actions                  POST         Add corrective or preventive action

  /api/v1/capas/{id}/verify                   PUT          Record effectiveness verification result
  -------------------------------------------------------------------------------------------------

**Chapter 12 --- Document Management Architecture**

**12.1 Document Domain Model**

  ---------------------------------------------------------------------------
  **Entity**           **Type**      **Description**
  -------------------- ------------- ----------------------------------------
  DocumentMaster       Aggregate     Document identity with type, owner,
                       Root          classification, and access control

  DocumentVersion      Entity        Individual revision with content,
                       (child)       approval status, and effective dates

  ApprovalWorkflow     Entity        Approval chain with step definitions,
                       (child)       current step, and completion status

  ApprovalStep         Value Object  Individual approval action with
                                     approver, decision, signature, and
                                     timestamp

  Distribution         Entity        Distribution list with acknowledgment
                       (child)       tracking per recipient

  DocumentAttachment   Value Object  Physical file reference with hash, size,
                                     and MIME type

  DocumentStatus       Enumeration   Draft, UnderReview, PendingApproval,
                                     Released, Superseded, Obsolete
  ---------------------------------------------------------------------------

**12.2 Version Control Rules**

Only one version of a document can hold Released status at any time.
When a new version is released, the previous version transitions to
Superseded status automatically. The released version\'s content is
immutable --- any change requires creating a new version. Draft versions
use minor numbering (1.1, 1.2) and released versions use major numbering
(2.0, 3.0). The system maintains the complete version history with full
content for each version, enabling point-in-time reconstruction of any
document state.

**12.3 Document Acknowledgment Architecture**

When a document transitions to Released, the system determines affected
personnel based on configurable rules: document type to role mapping,
department associations, and explicit distribution lists. Each affected
user receives an in-app notification and (optionally) email
notification. Acknowledgment requires the user to confirm they have read
and understood the document. Unacknowledged documents after the
compliance deadline trigger escalation to the user\'s supervisor.
Acknowledgment completion statistics feed into training compliance
dashboards.

**Chapter 13 --- Audit Management Architecture**

**13.1 Audit Domain Model**

  ------------------------------------------------------------------------------
  **Entity**              **Type**      **Description**
  ----------------------- ------------- ----------------------------------------
  AuditPlan               Aggregate     Annual or periodic audit plan with
                          Root          scope, schedule, and resource allocation

  AuditRecord             Entity        Individual audit execution with auditor
                          (child)       assignment and status

  AuditChecklist          Entity        Checklist bound to a standard (ISO 9001,
                          (child)       IATF, AS9100) with clause references

  AuditFinding            Entity        Individual finding with classification,
                          (child)       evidence, and corrective action linkage

  AuditReport             Value Object  Generated audit report with summary,
                                        findings, and recommendations

  FindingClassification   Enumeration   Major NC, Minor NC, Observation,
                                        Opportunity for Improvement
  ------------------------------------------------------------------------------

**13.2 Audit Planning and Coverage Matrix**

The audit planning module maintains a coverage matrix that maps
processes and areas against audit occurrences. The matrix ensures
complete coverage of all quality system processes within the audit
cycle. Risk-based prioritization assigns higher audit frequency to
processes with recent non-conformances, customer complaints, or
significant changes. The coverage matrix generates visual gap reports
showing which processes are overdue for audit.

**13.3 Finding-to-CAPA Integration**

Audit findings classified as Major NC or Minor NC emit an
AuditFindingRecorded domain event. The CAPA module subscribes to this
event and, depending on tenant configuration, either auto-creates a CAPA
record linked to the finding or queues the finding for manual CAPA
initiation by the quality manager. This ensures that no significant
audit finding can be closed without a corrective action trail.

**Chapter 14 --- Statistical Process Control Architecture**

**14.1 SPC Engine Design**

The SPC module consumes measurement data from the Quality Inspection
module through InspectionCompleted domain events. Each measurement that
matches a configured SPC characteristic is appended to the corresponding
control chart data series. Control limits are calculated using standard
statistical methods and recalculated on a configurable schedule (or
manually triggered by an engineer).

**14.2 Supported Chart Types**

  ------------------------------------------------------------------------------
  **Chart Type**      **Data Type**      **Application**     **Control Limit
                                                             Method**
  ------------------- ------------------ ------------------- -------------------
  X-bar/R             Variables          Process mean and    A2, D3, D4 factors
                      (subgroups 2--10)  range monitoring    

  X-bar/S             Variables          Process mean and    A3, B3, B4 factors
                      (subgroups \> 10)  std dev monitoring  

  Individual/Moving   Variables (single  Low-volume or batch E2, D3, D4 factors
  Range               observations)      processes           

  p chart             Attributes         Defective rate      Based on average
                      (proportion        monitoring          proportion
                      defective)                             

  np chart            Attributes (count  Defective count     Based on average
                      defective,         monitoring          count
                      constant n)                            

  c chart             Attributes         Defect count per    Based on average
                      (defects per unit, unit                count
                      constant area)                         

  u chart             Attributes         Defect rate         Based on average
                      (defects per unit, normalization       rate
                      variable area)                         
  ------------------------------------------------------------------------------

**14.3 Out-of-Control Detection Rules**

The SPC engine implements Western Electric rules and Nelson rules for
automated out-of-control detection. When a rule violation is detected,
the system: marks the data point on the chart with the specific rule
violated; emits an SPCViolationDetected domain event; sends
notifications to configured recipients (operator, supervisor, engineer);
and optionally triggers automatic inspection tightening for the affected
characteristic.

**14.4 Process Capability Analysis**

The module calculates Cp, Cpk (short-term capability), Pp, and Ppk
(long-term performance) indices. Capability indices are displayed
alongside control charts and tracked over time to show process
improvement trends. Capability reports can be generated per part, per
characteristic, per machine, and per time period --- supporting
customer-required capability studies and PPAP submissions.

**Chapter 15 --- Supplier Quality Architecture**

This chapter provides the complete implementation architecture for the
Supplier Quality bounded context. The original business requirements
(Chapter 4) defined scorecards, SCAR workflow, and approved supplier
list capabilities. This chapter specifies the database schema, API
surface, state machines, portal architecture, and integration patterns
required for development.

**15.1 Supplier Quality Domain Model**

  -------------------------------------------------------------------------------
  **Entity**               **Type**      **Description**
  ------------------------ ------------- ----------------------------------------
  Supplier                 Aggregate     Supplier master record with
                           Root          classification, approval status, and
                                         contact information

  SupplierApproval         Entity        Approval record with scope
                           (child)       (parts/commodities), approval date,
                                         expiry, and conditions

  SupplierScorecard        Entity        Periodic performance scorecard with
                           (child)       weighted category scores

  ScorecardCategory        Value Object  Individual scoring category (Quality,
                                         Delivery, Cost, Responsiveness) with
                                         weight and score

  SCARRecord               Entity        Supplier Corrective Action Request
                           (child)       lifecycle

  SCARResponse             Value Object  Supplier\'s response with root cause,
                                         corrective actions, and evidence

  SupplierContact          Value Object  Named contact with role, email, phone
                                         for notification routing

  ApprovedPartList         Entity        Parts this supplier is approved to
                           (child)       provide with revision scope

  SupplierRiskAssessment   Value Object  Risk classification
                                         (Low/Medium/High/Critical) with
                                         contributing factors

  SupplierStatus           Enumeration   Prospective, Approved,
                                         ConditionalApproval, OnProbation,
                                         Disqualified, Inactive
  -------------------------------------------------------------------------------

**15.2 SCAR Workflow State Machine**

***Diagram: SCAR Lifecycle State Machine***

stateDiagram-v2 \[\*\] \--\> Issued : SCARCreated Issued \--\>
AwaitingResponse : SentToSupplier AwaitingResponse \--\> UnderReview :
SupplierResponded AwaitingResponse \--\> Overdue :
ResponseDeadlineExceeded Overdue \--\> UnderReview :
LateResponseReceived UnderReview \--\> Accepted : ResponseAdequate
UnderReview \--\> Rejected : ResponseInadequate Rejected \--\>
AwaitingResponse : ReissuedToSupplier Accepted \--\> FollowUp :
VerificationRequired FollowUp \--\> Closed : VerificationPassed FollowUp
\--\> Rejected : VerificationFailed Closed \--\> \[\*\]

**15.2.1 SCAR Business Rules**

**Response Deadline:** Default fourteen calendar days from issuance.
Configurable per supplier tier (strategic suppliers may receive
twenty-one days). System sends automated reminders at seven days, three
days, and one day before deadline.

**Escalation on Overdue:** If response deadline passes without a
supplier response, the system notifies the SQE, escalates to Quality
Manager after seven additional days, and flags the supplier scorecard
with a responsiveness penalty.

**Repeat SCAR Detection:** When a new SCAR is created, the system checks
for previous SCARs to the same supplier for the same defect category
within the past twelve months. Repeat SCARs are flagged and may trigger
automatic probation review.

**Scorecard Impact:** SCAR closure updates the supplier scorecard
automatically. Accepted SCARs with verified corrective actions have
neutral impact. Rejected or overdue SCARs apply weighted penalties to
the Quality and Responsiveness categories.

**15.3 Supplier Scorecard Architecture**

**15.3.1 Scorecard Calculation Model**

  -------------------------------------------------------------------------------------
  **Category**     **Weight**   **Data Sources**         **Calculation Method**
  ---------------- ------------ ------------------------ ------------------------------
  Quality          40%          IQC inspection results   Weighted composite: 60% lot
                                (lot accept/reject rate, acceptance rate + 25% defect
                                defect PPM), NC count by PPM performance vs target +
                                severity                 15% NC severity index

  Delivery         25%          Purchase order on-time   On-time percentage weighted by
                                delivery rate, quantity  PO value
                                accuracy                 

  Responsiveness   20%          SCAR response time, SCAR Average response days vs
                                closure rate,            target + closure rate within
                                communication timeliness deadline

  Cost             15%          Price competitiveness,   Normalized index against
                                cost reduction           commodity benchmark
                                contributions, warranty  
                                claim rate               
  -------------------------------------------------------------------------------------

**15.3.2 Scorecard Schedule**

Scorecards are calculated monthly with a rolling twelve-month data
window. The calculation runs as a scheduled Hangfire job on the first
business day of each month. Quarterly summaries aggregate monthly scores
and trigger supplier tier reviews. Annual reviews determine approval
renewal, probation, or disqualification decisions. Historical scorecards
are immutable once published --- corrections create a new scorecard
entry with an amendment reference.

**15.4 Supplier Performance Monitoring**

Real-time supplier performance monitoring is implemented through domain
event consumption. The Supplier Quality module subscribes to
InspectionCompleted events for IQC inspections, NonConformanceRaised
events with supplier origin, and SCARStatusChanged events. Each event
updates running performance accumulators in the supplier analytics
store.

  ------------------------------------------------------------------------
  **Metric**    **Calculation**       **Alert         **Action on Breach**
                                      Threshold**     
  ------------- --------------------- --------------- --------------------
  Lot Rejection Rejected lots / total \> 5%           Automatic
  Rate          lots received                         notification to SQE;
                (rolling 3 months)                    probation review at
                                                      \> 10%

  Defect PPM    Defective parts /     Above agreed    SCAR auto-generation
                total parts received  target PPM      above 2x target
                \* 1,000,000                          

  SCAR Response On-time responses /   \< 80%          Supplier tier
  Rate          total SCARs issued                    downgrade review

  Repeat Defect Same defect category  \> 2            Mandatory supplier
  Rate          recurrence within 12  occurrences     audit, probation
                months                                review

  Critical NC   Critical NCs          \> 0            Immediate
  Count         attributed to                         containment,
                supplier (rolling 12                  management
                months)                               notification
  ------------------------------------------------------------------------

**15.5 Supplier Portal Architecture**

The Supplier Portal is a restricted-access web interface that allows
approved supplier contacts to view their scorecards, respond to SCARs,
acknowledge document distributions, and update their quality
certifications. The portal is a separate frontend application that
authenticates against the Berex Tech - QMS Identity module using a
dedicated supplier role with strictly scoped permissions.

**15.5.1 Portal Capabilities**

  -----------------------------------------------------------------------
  **Capability**   **Description**                **Data Access Scope**
  ---------------- ------------------------------ -----------------------
  Scorecard View   View own monthly, quarterly,   Read-only, own supplier
                   and annual scorecards          record only

  SCAR Response    View issued SCARs, submit root Read-write on own SCARs
                   cause analysis and corrective  only
                   actions, upload evidence       

  Document         Receive and acknowledge        Read-only, documents
  Acknowledgment   distributed quality documents  distributed to this
                                                  supplier

  Certificate      Upload updated quality         Write to own
  Upload           certifications (ISO, IATF,     certification records
                   etc.)                          

  Performance      View trend charts of own       Read-only, own metrics
  Dashboard        quality metrics                only
  -----------------------------------------------------------------------

**15.5.2 Portal Security**

Supplier portal users authenticate via a separate JWT flow with
supplier-scoped claims. All portal API endpoints enforce tenant
isolation plus supplier isolation --- a supplier user can only access
records where supplier_id matches their token claim. Rate limiting is
applied more aggressively on portal endpoints (sixty requests per
minute) than internal endpoints. All portal activity is logged to the
audit trail with a source marker indicating external access.

**15.6 Supplier Quality Database Schema**

  ------------------------------------------------------------------------------------
  **Table**                     **Key Columns**                 **Indexes**
  ----------------------------- ------------------------------- ----------------------
  supplier.suppliers            id, tenant_id, code, name,      (tenant_id, code)
                                status, risk_level, tier,       UNIQUE; (tenant_id,
                                approved_since, created_at      status)

  supplier.supplier_approvals   id, supplier_id,                (supplier_id,
                                scope_description,              expiry_date)
                                approved_date, expiry_date,     
                                conditions                      

  supplier.scorecards           id, supplier_id, period_start,  (supplier_id,
                                period_end, quality_score,      period_start) UNIQUE
                                delivery_score,                 
                                responsiveness_score,           
                                cost_score, overall_score,      
                                status                          

  supplier.scar_records         id, supplier_id, nc_id,         (supplier_id, status);
                                issued_date, response_deadline, (nc_id)
                                status, severity                

  supplier.scar_responses       id, scar_id, response_date,     (scar_id)
                                root_cause, corrective_actions, 
                                evidence_refs                   

  supplier.approved_parts       id, supplier_id, part_id,       (supplier_id, part_id)
                                revision_scope, approval_date,  UNIQUE
                                status                          
  ------------------------------------------------------------------------------------

**15.7 Supplier Quality API Surface**

  ----------------------------------------------------------------------------------------------
  **Endpoint**                                 **Method**   **Description**
  -------------------------------------------- ------------ ------------------------------------
  /api/v1/suppliers                            GET/POST     List or create supplier records

  /api/v1/suppliers/{id}                       GET/PUT      Get or update supplier details

  /api/v1/suppliers/{id}/scorecard             GET          Get current and historical
                                                            scorecards

  /api/v1/suppliers/{id}/scorecard/calculate   POST         Trigger manual scorecard
                                                            recalculation

  /api/v1/scars                                POST         Issue new SCAR (from NC or
                                                            standalone)

  /api/v1/scars/{id}                           GET/PUT      Get SCAR detail or update status

  /api/v1/scars/{id}/respond                   POST         Submit supplier response (portal
                                                            endpoint)

  /api/v1/scars/{id}/review                    PUT          Accept or reject supplier response

  /api/v1/scars/{id}/verify                    PUT          Record follow-up verification result

  /api/v1/suppliers/{id}/performance           GET          Get real-time performance metrics

  /api/v1/portal/my-scorecards                 GET          Supplier portal: view own scorecards

  /api/v1/portal/my-scars                      GET          Supplier portal: view own SCARs
  ----------------------------------------------------------------------------------------------

**Chapter 16 --- Calibration and Metrology Architecture**

This chapter provides the complete implementation architecture for the
Calibration and Metrology bounded context. The integrity gate system in
Quality Inspection depends on this module to validate that measurement
equipment has a current, valid calibration before inspection data entry
is permitted.

**16.1 Equipment Registry**

  -------------------------------------------------------------------------------
  **Entity**               **Type**      **Description**
  ------------------------ ------------- ----------------------------------------
  Equipment                Aggregate     Measurement equipment master record with
                           Root          identification, type, location, and
                                         status

  CalibrationRecord        Entity        Individual calibration event with
                           (child)       results, adjustments, and technician

  CalibrationCertificate   Value Object  Certificate document reference with
                                         issuing lab, date, and validity period

  CalibrationSchedule      Entity        Scheduling rules including frequency,
                           (child)       next due date, and lead time

  GaugeControl             Entity        Gauge R&R study results, measurement
                           (child)       capability assessment

  EquipmentAssignment      Value Object  Department/area assignment with custody
                                         tracking

  EquipmentStatus          Enumeration   Active, DueForCalibration, Overdue,
                                         OutOfService, InCalibration, Retired

  CalibrationResult        Enumeration   Pass, PassWithAdjustment, Fail, Limited
  -------------------------------------------------------------------------------

**16.2 Calibration Scheduling**

Each equipment record has a calibration schedule defining the
calibration interval (in days), the responsible laboratory (internal or
external), the required calibration procedure, and the lead time for
scheduling. The scheduling engine runs as a daily Hangfire job that
evaluates all active equipment and manages status transitions.

  -----------------------------------------------------------------------
  **Schedule    **Trigger**            **System Action**
  Event**                              
  ------------- ---------------------- ----------------------------------
  Approaching   Equipment due date     Notify Calibration Owner; create
  Due           within lead time       calibration work order if internal
                window                 

  Due Today     Equipment due date     Status transitions to
                equals current date    DueForCalibration; notification
                                       escalation

  Overdue       Equipment due date has Status transitions to Overdue;
                passed without         equipment blocked from inspection
                completed calibration  use; management notification

  Calibration   Technician begins      Status transitions to
  Started       calibration            InCalibration; equipment removed
                                       from available pool

  Calibration   Technician records     Status transitions to Active; next
  Completed     results and uploads    due date calculated; equipment
                certificate            returned to available pool

  Calibration   Equipment fails        Status transitions to
  Failed        calibration criteria   OutOfService; triggers escaped
                                       measurement analysis; management
                                       notification
  -----------------------------------------------------------------------

**16.3 Certificate Management**

Calibration certificates are stored as immutable document attachments
linked to the calibration record. Each certificate captures: issuing
laboratory name and accreditation reference, calibration date,
calibration procedure reference, environmental conditions during
calibration, measurement results with uncertainties, traceability chain
to national/international standards, pass/fail determination, and next
calibration due date. Certificates from external laboratories can be
uploaded as PDF attachments. Internal calibration certificates are
generated by the system from recorded calibration data.

**16.4 Gauge Control and R&R Studies**

The Gauge Control subsystem manages Gauge Repeatability and
Reproducibility (Gauge R&R) studies per AIAG MSA manual guidelines. Each
measurement system used for critical characteristics requires a Gauge
R&R study demonstrating acceptable measurement capability. Study results
are stored per equipment-characteristic combination and include: total
Gauge R&R percentage, repeatability (equipment variation),
reproducibility (appraiser variation), part variation, number of
distinct categories (ndc), and pass/fail determination against
configurable acceptance criteria (typically less than ten percent Gauge
R&R for critical, less than thirty percent for non-critical).

**16.5 Escaped Measurement Impact Analysis**

When equipment fails calibration, all inspections performed with that
equipment since the last successful calibration become suspect. The
Escaped Measurement Impact Analysis identifies affected inspection
records and triggers a structured review process.

***Diagram: Escaped Measurement Impact Flow***

sequenceDiagram participant CalTech as Calibration Technician
participant CalModule as Calibration Module participant InspModule as
Inspection Module participant QualMgr as Quality Manager
CalTech-\>\>CalModule: Record calibration failure
CalModule-\>\>CalModule: Identify affected date range (last pass to now)
CalModule-\>\>InspModule: Query inspections using this equipment in date
range InspModule\--\>\>CalModule: Return affected inspection list
CalModule-\>\>CalModule: Generate impact assessment report
CalModule-\>\>QualMgr: Notify with impact assessment
QualMgr-\>\>InspModule: Review and disposition affected inspections

**16.6 Calibration Database Schema**

  -------------------------------------------------------------------------------------------
  **Table**                         **Key Columns**                 **Indexes**
  --------------------------------- ------------------------------- -------------------------
  calibration.equipment             id, tenant_id, code, name,      (tenant_id, code) UNIQUE;
                                    type, manufacturer, model,      (tenant_id, status)
                                    serial_number, status,          
                                    location, custodian_id          

  calibration.calibration_records   id, equipment_id,               (equipment_id,
                                    calibration_date, result,       calibration_date);
                                    technician_id, procedure_ref,   (next_due_date)
                                    next_due_date, certificate_id   

  calibration.certificates          id, calibration_record_id,      (calibration_record_id)
                                    issuing_lab, accreditation_ref, 
                                    file_ref, valid_from,           
                                    valid_until                     

  calibration.schedules             id, equipment_id,               (equipment_id) UNIQUE;
                                    interval_days, lead_time_days,  (next_due_date)
                                    lab_type, procedure_ref,        
                                    next_due_date                   

  calibration.gauge_rr_studies      id, equipment_id,               (equipment_id,
                                    characteristic_id, study_date,  characteristic_id)
                                    total_grr_pct,                  
                                    repeatability_pct,              
                                    reproducibility_pct, ndc,       
                                    result                          

  calibration.impact_assessments    id, equipment_id,               (equipment_id,
                                    failed_cal_id, affected_from,   failed_cal_id)
                                    affected_to,                    
                                    affected_inspection_count,      
                                    status, reviewed_by             
  -------------------------------------------------------------------------------------------

**16.7 Calibration API Surface**

  ----------------------------------------------------------------------------------------------------------
  **Endpoint**                                              **Method**   **Description**
  --------------------------------------------------------- ------------ -----------------------------------
  /api/v1/equipment                                         GET/POST     List or register equipment

  /api/v1/equipment/{id}                                    GET/PUT      Get or update equipment details

  /api/v1/equipment/{id}/calibrations                       GET/POST     List or record calibration events

  /api/v1/equipment/{id}/calibrations/{calId}/certificate   POST         Upload calibration certificate

  /api/v1/equipment/{id}/gauge-rr                           GET/POST     List or record Gauge R&R studies

  /api/v1/equipment/{id}/status                             GET          Get current calibration status
                                                                         (used by integrity gates)

  /api/v1/calibration/schedule                              GET          Get calibration schedule dashboard

  /api/v1/calibration/overdue                               GET          List overdue equipment

  /api/v1/calibration/impact-assessment/{id}                GET/PUT      Get or update escaped measurement
                                                                         impact assessment
  ----------------------------------------------------------------------------------------------------------

**Chapter 17 --- Training and Competency Architecture**

This chapter provides the complete implementation architecture for the
Training and Competency bounded context. The integrity gate system in
Quality Inspection depends on this module to validate that inspectors
hold current qualifications before inspection data entry is permitted.

**17.1 Training Domain Model**

  ----------------------------------------------------------------------------
  **Entity**            **Type**      **Description**
  --------------------- ------------- ----------------------------------------
  Employee              Aggregate     Employee profile linked to Identity
                        Root          module user record, with department and
                        (reference)   job role

  Qualification         Entity        Defined qualification type with scope
                                      (product family, inspection type,
                                      process area)

  CompetencyRecord      Entity (child Individual competency achievement
                        of Employee)  linking employee to qualification with
                                      evidence

  TrainingCourse        Entity        Training course definition with content,
                                      duration, assessment criteria, and
                                      expiry rules

  TrainingAssignment    Entity        Assignment of a course to an employee
                                      with due date and completion tracking

  TrainingCompletion    Value Object  Completion record with date, score,
                                      assessor, and evidence reference

  SkillMatrix           Read Model    Cross-reference view of employees versus
                                      qualifications for coverage analysis

  QualificationStatus   Enumeration   NotStarted, InTraining, Qualified,
                                      Expired, Suspended, Revoked
  ----------------------------------------------------------------------------

**17.2 Competency Lifecycle**

***Diagram: Competency Lifecycle State Machine***

stateDiagram-v2 \[\*\] \--\> NotQualified : EmployeeAssigned
NotQualified \--\> InTraining : TrainingStarted InTraining \--\>
PendingAssessment : TrainingCompleted PendingAssessment \--\> Qualified
: AssessmentPassed PendingAssessment \--\> InTraining : AssessmentFailed
Qualified \--\> Expiring : WithinRenewalWindow Expiring \--\> InTraining
: RenewalTrainingStarted Expiring \--\> Expired : RenewalDeadlinePassed
Expired \--\> InTraining : RetrainingAssigned Qualified \--\> Suspended
: PerformanceIssue Suspended \--\> InTraining : RetrainingRequired
Qualified \--\> Revoked : PolicyViolation

**17.2.1 Competency Business Rules**

**Qualification Expiry:** Each qualification type has a configurable
validity period (typically twelve to twenty-four months). The system
tracks expiry dates and triggers renewal notifications at ninety, sixty,
and thirty days before expiry.

**Renewal Window:** A configurable renewal window (default sixty days
before expiry) allows employees to complete renewal training before
their qualification lapses. If renewal is completed within the window,
there is no gap in qualification.

**Expiry Impact:** When a qualification expires, the system emits a
QualificationExpired domain event. The Inspection module subscribes to
this event and updates its integrity gate validation cache. Any
inspection attempt by the expired employee for the affected scope will
be blocked until requalification.

**Cascade from CAPA:** CAPA actions that identify training gaps can
auto-generate training assignments. The CAPA module emits a
TrainingGapIdentified event, and the Training module creates the
appropriate assignment.

**17.3 Qualification Validation for Integrity Gates**

The integrity gate validation query is performance-critical --- it runs
on every inspection start. The Training module exposes a synchronous API
endpoint that the Inspection module calls during gate validation. The
endpoint accepts employee_id, product_family, and inspection_type, and
returns a boolean qualified status with the qualification expiry date if
qualified. To minimize latency, qualification status is cached in Redis
with a TTL matching the shortest remaining validity period. Cache
invalidation occurs on any qualification state change event.

**17.4 Skill Matrix Architecture**

The Skill Matrix is a read model (CQRS query side) that provides a
cross-tabulation of employees versus qualifications. Each cell shows the
qualification status (Qualified, Expiring, Expired, Not Qualified) with
color coding for visual gap analysis. The matrix supports filtering by
department, product family, and inspection type. It enables managers to
identify coverage gaps (a product family where only one inspector is
qualified --- single point of failure) and plan training accordingly.

  -------------------------------------------------------------------------
  **Matrix         **Source**                     **Filter Options**
  Dimension**                                     
  ---------------- ------------------------------ -------------------------
  Rows: Employees  Identity module (active        Department, job role,
                   employees in quality roles)    site

  Columns:         Training module (active        Product family,
  Qualifications   qualification definitions)     inspection type, process
                                                  area

  Cell: Status     CompetencyRecord for           Status filter (show only
                   employee-qualification pair    gaps, show only expiring)
  -------------------------------------------------------------------------

**17.5 Document Acknowledgment Integration**

When the Document Control module releases a document that affects
quality procedures or work instructions, the Training module may need to
verify that affected personnel have acknowledged the change. The
integration works through the DocumentApproved domain event: the
Training module evaluates whether the document type and scope map to any
qualification requirements, and if so, generates acknowledgment tasks or
retraining assignments as appropriate. Document acknowledgment
completion feeds into the competency record as evidence of continued
awareness.

**17.6 Training Database Schema**

  --------------------------------------------------------------------------------------
  **Table**                       **Key Columns**                 **Indexes**
  ------------------------------- ------------------------------- ----------------------
  training.qualifications         id, tenant_id, code, name,      (tenant_id, code)
                                  scope_product_family,           UNIQUE
                                  scope_inspection_type,          
                                  validity_months,                
                                  renewal_window_days             

  training.competency_records     id, employee_id,                (employee_id,
                                  qualification_id, status,       qualification_id)
                                  qualified_date, expiry_date,    UNIQUE; (expiry_date);
                                  assessor_id, evidence_ref       (status)

  training.courses                id, tenant_id, code, name,      (tenant_id, code)
                                  duration_hours,                 UNIQUE
                                  assessment_type, pass_criteria, 
                                  qualification_ids               

  training.training_assignments   id, employee_id, course_id,     (employee_id, status);
                                  assigned_by, assigned_date,     (due_date)
                                  due_date, status                

  training.training_completions   id, assignment_id,              (assignment_id)
                                  completion_date, score,         
                                  assessor_id, result,            
                                  evidence_ref                    

  training.skill_matrix_cache     employee_id, qualification_id,  (employee_id,
                                  status, expiry_date,            qualification_id) PK;
                                  last_updated                    refreshed via
                                                                  materialized view
  --------------------------------------------------------------------------------------

**17.7 Training API Surface**

  ------------------------------------------------------------------------------------------------
  **Endpoint**                                   **Method**   **Description**
  ---------------------------------------------- ------------ ------------------------------------
  /api/v1/qualifications                         GET/POST     List or create qualification
                                                              definitions

  /api/v1/qualifications/{id}                    GET/PUT      Get or update qualification details

  /api/v1/employees/{id}/competencies            GET          Get all competency records for an
                                                              employee

  /api/v1/employees/{id}/competencies/validate   GET          Validate qualification for integrity
                                                              gate (cached)

  /api/v1/training/courses                       GET/POST     List or create training courses

  /api/v1/training/assignments                   GET/POST     List or create training assignments

  /api/v1/training/assignments/{id}/complete     PUT          Record training completion with
                                                              assessment result

  /api/v1/training/skill-matrix                  GET          Get skill matrix with filtering
                                                              options

  /api/v1/training/expiring                      GET          List qualifications expiring within
                                                              window

  /api/v1/training/gaps                          GET          Identify coverage gaps in skill
                                                              matrix
  ------------------------------------------------------------------------------------------------

**Chapter 18 --- AI/ML Integration Architecture**

**18.1 AI Contract with Users**

The AI capabilities in Berex Tech - QMS operate under a strict contract:
AI assists, it never decides. No AI output triggers a state change in
any quality record without explicit human confirmation. AI suggestions
are always presented with confidence scores and source references
(grounded in Berex\'s own historical data). The AI is an accelerator for
experienced quality professionals, not a replacement for engineering
judgment.

The AI engine maintains its own denormalized analytical data store
populated through event-driven data ingestion from upstream modules. All
AI capabilities are exposed as asynchronous services that enrich the
user experience without blocking critical quality processes.

**18.2 AI Capability Inventory**

  --------------------------------------------------------------------------
  **Capability**   **Description**     **Input Data**      **Model Type**
  ---------------- ------------------- ------------------- -----------------
  Defect           Predict probability Inspection results, Gradient Boosted
  Prediction       of defect           process parameters, Trees / Neural
                   occurrence based on environmental data  Network
                   process parameters                      

  Anomaly          Identify unusual    SPC measurements,   Isolation Forest
  Detection        patterns in quality inspection trends,  / Autoencoder
                   data streams        NC frequency        

  Root Cause       Suggest probable    NC descriptions,    NLP +
  Suggestion       root causes for     defect codes,       Classification
                   non-conformances    historical RCA      
                                       outcomes            

  Document         Auto-classify and   Document text       NLP Transformer
  Classification   tag documents based content, metadata   (fine-tuned)
                   on content analysis                     

  Supplier Risk    Predict supplier    Supplier            Logistic
  Scoring          quality risk based  scorecards, IQC     Regression /
                   on performance data results, NC history Random Forest

  Inspection       Recommend sampling  Historical          Bayesian
  Optimization     plan adjustments    inspection results, Optimization
                   based on quality    defect rates,       
                   history             capability          
  --------------------------------------------------------------------------

**18.3 AI Architecture**

***Diagram: AI Engine Architecture***

graph TB subgraph Data Sources A\[Inspection Results\] B\[NC Records\]
C\[CAPA Data\] D\[Audit Findings\] E\[SPC Data\] F\[Document Content\]
end subgraph Event Bus G\[Domain Events\] end subgraph AI Engine H\[Data
Ingestion Pipeline\] I\[Feature Store\] J\[Model Registry\]
K\[Prediction Service\] L\[Training Pipeline\] M\[NLP Service\] end
subgraph Consumers N\[Dashboards\] O\[Alert System\] P\[Decision
Support\] end A & B & C & D & E & F \--\> G G \--\> H \--\> I I \--\> L
\--\> J J \--\> K K \--\> N & O & P F \--\> M \--\> P

**18.4 Model Lifecycle Management**

The AI Engine implements a structured model lifecycle: data preparation
with feature engineering, model training with hyperparameter tuning,
validation against held-out test sets, champion-challenger deployment
(new model runs in shadow mode alongside the current production model),
promotion based on performance metrics comparison, monitoring for data
drift and model degradation, and retirement with archival. All lifecycle
transitions are recorded in the model registry with full reproducibility
metadata (data snapshot reference, hyperparameters, training metrics,
validation metrics).

**18.5 Staged AI Rollout**

AI capabilities are introduced in stages aligned with data availability.
The AI RCA Assistant is deliberately deferred to development Phase 4,
not Phase 1. An AI that suggests root causes with no Berex defect
history gives textbook answers, and quality engineers will dismiss it
permanently. The rollout plan: Phase 1 deploys no AI (focus on data
foundation). Phase 2 enables document Q&A and report drafting (works
with existing documents). Phase 3 enables anomaly detection and basic
trend analysis (needs six-plus months of inspection data). Phase 4
enables RCA suggestion and defect prediction (needs nine-plus months of
structured defect data with RCA outcomes).

**18.6 LLM Integration Architecture**

Large Language Model integration follows a provider-abstraction pattern.
The LLM Adapter interface defines methods for text completion, embedding
generation, and structured extraction. Concrete implementations exist
for OpenAI, Azure OpenAI, and a local model variant (for organizations
that cannot send data externally). The adapter selection is
configuration-driven at deployment time. The RAG (Retrieval-Augmented
Generation) pipeline grounds every LLM response in Berex\'s own quality
documents and historical data --- the system never presents
LLM-generated content without source attribution.

**Permission-Filtered RAG:** The RAG retrieval layer enforces the same
RBAC permissions as the source modules. A user querying the AI assistant
can only receive information grounded in documents and records they have
permission to access. This prevents information leakage across role
boundaries through the AI channel.

**Chapter 19 --- AI Governance and Safety**

This chapter establishes the governance framework, safety controls, and
operational policies for all AI capabilities within Berex Tech - QMS.
These controls are mandatory for any AI feature deployment and are
designed for a regulated manufacturing quality environment where AI
outputs may influence decisions affecting product safety.

**19.1 Human Approval Requirement**

No AI output shall trigger a state change in any quality record without
explicit human confirmation. This is enforced architecturally, not by
policy alone.

  ------------------------------------------------------------------------
  **Rule**        **Implementation**             **Verification**
  --------------- ------------------------------ -------------------------
  AI suggestions  AI service endpoints return    Code review checklist
  are read-only   suggestion payloads only; no   item; integration test
                  AI endpoint has write access   verifies AI endpoints
                  to quality record tables       cannot modify quality
                                                 records

  Human           Every AI suggestion presented  UI automation test
  confirmation    in the UI includes explicit    verifies no auto-accept
  required        Accept/Reject controls;        path exists
                  acceptance creates a           
                  human-authored action with AI  
                  suggestion as reference        

  Audit trail     When a user accepts an AI      Audit log schema enforces
  attribution     suggestion, the audit log      non-null human_user_id on
                  records both the human         all quality record
                  decision and the AI suggestion mutations
                  ID, clearly distinguishing     
                  human action from AI input     

  No autonomous   AI-detected anomalies generate Event handler code
  escalation      notifications for human        review; no AI event
                  review; they do not            handler writes to quality
                  automatically trigger quality  domain tables
                  holds, supplier downgrades, or 
                  process changes                
  ------------------------------------------------------------------------

**19.2 Confidence Threshold Framework**

Every AI prediction or suggestion includes a confidence score (0.0 to
1.0). The system enforces configurable confidence thresholds that
determine how suggestions are presented to users.

  --------------------------------------------------------------------------------------
  **Confidence   **Presentation**    **User Action       **Audit Classification**
  Range**                            Required**          
  -------------- ------------------- ------------------- -------------------------------
  0.0 -- 0.30    Suggestion          None (suggestion    Logged as
  (Low)          suppressed; not     discarded)          suppressed_low_confidence in AI
                 shown to user                           audit trail

  0.31 -- 0.60   Presented with      Explicit acceptance Logged as
  (Moderate)     amber indicator and with mandatory      moderate_confidence_accepted or
                 disclaimer: \'Low   justification       \_rejected
                 confidence ---                          
                 verify                                  
                 independently\'                         

  0.61 -- 0.85   Presented with      Explicit acceptance Logged as
  (High)         green indicator and or rejection        high_confidence_accepted or
                 supporting evidence                     \_rejected

  0.86 -- 1.00   Presented as        Explicit acceptance Logged as
  (Very High)    primary suggestion  or rejection (no    very_high_confidence_accepted
                 with supporting     auto-accept)        or \_rejected
                 evidence                                
  --------------------------------------------------------------------------------------

Confidence thresholds are tenant-configurable within defined safety
bounds. The low-confidence suppression threshold cannot be set below
0.20, and no threshold configuration can enable auto-acceptance of AI
suggestions.

**19.3 AI Hallucination Prevention**

**Grounding requirement:** Every AI-generated text response must be
grounded in retrievable source data from Berex Tech - QMS. The RAG
pipeline enforces this by requiring at least one source document
reference for any generated response. If the retrieval step returns no
relevant sources, the AI responds with \'Insufficient data to provide a
suggestion\' rather than generating from its general training.

**Source attribution:** AI responses include clickable references to the
source records (NC reports, CAPA records, inspection data, documents)
that grounded the response. Users can verify any AI claim against the
original data.

**Factual constraint layer:** For quantitative outputs (defect rates,
capability indices, trend projections), the AI service validates
computed values against the source data before returning them. If the
computed value diverges from the source data by more than a configurable
tolerance, the output is flagged as potentially unreliable.

**Prohibition on fabrication:** The system prompt for all LLM
interactions includes an explicit instruction to respond \'I don\'t have
enough information\' rather than speculate. This instruction is embedded
in the system architecture (not just the prompt) through the
source-retrieval requirement above.

**19.4 AI Audit Trail**

Every AI interaction is recorded in a dedicated AI audit trail, separate
from the main audit_log. This enables AI-specific compliance reporting
and model performance analysis.

  -------------------------------------------------------------------------
  **Field**            **Description**
  -------------------- ----------------------------------------------------
  ai_interaction_id    Unique identifier for this AI interaction

  tenant_id            Tenant context

  user_id              User who requested or received the AI output

  capability           AI capability invoked (defect_prediction,
                       rca_suggestion, etc.)

  model_id             Model version used (from model registry)

  input_summary        Hash or summary of input data (not raw data for
                       privacy)

  output_summary       AI output summary with confidence score

  confidence_score     Numerical confidence (0.0 to 1.0)

  source_references    Array of source record IDs that grounded the output

  user_action          accepted, rejected, modified, ignored, suppressed

  user_justification   User-provided reason (mandatory for
                       moderate-confidence acceptance)

  timestamp            UTC timestamp of interaction

  response_time_ms     Latency of AI service response
  -------------------------------------------------------------------------

**19.5 AI Fallback Behaviour**

AI capabilities are designed to degrade gracefully. The system must
function fully without AI --- AI enhances the experience but is never on
the critical path for any quality process.

  -----------------------------------------------------------------------
  **Failure        **Fallback Behaviour**         **User Impact**
  Scenario**                                      
  ---------------- ------------------------------ -----------------------
  AI service       AI suggestion panels show \'AI Users proceed with
  unavailable      temporarily unavailable\' with manual RCA, manual
                   manual-only workflow           classification; no
                                                  process blockage

  LLM provider     Document Q&A and report        Reduced NLP
  outage           drafting disabled; all other   functionality; core
                   AI capabilities from local     prediction and anomaly
                   models continue                detection unaffected

  Model training   Current production model       No user impact; model
  failure          continues serving; failed      refresh delayed until
                   training logged for            next successful
                   investigation                  training

  Data ingestion   AI operates on last            Suggestions may not
  lag              successfully ingested data;    reflect very recent
                   staleness indicator shown to   data; user notified of
                   users                          data currency

  Confidence below No suggestion shown; user      No user impact; AI
  all thresholds   proceeds with manual process   transparently steps
                                                  back when it cannot
                                                  help
  -----------------------------------------------------------------------

**19.6 AI Operational Safety**

**Kill switch:** Each AI capability has an independent enable/disable
toggle accessible to the Quality Manager. Disabling a capability takes
effect immediately and redirects all users to manual workflows. No AI
capability can be enabled without Quality Manager approval.

**Rate limiting:** AI service calls are rate-limited per user (maximum
thirty requests per minute) and per tenant (maximum five hundred
requests per minute) to prevent abuse and manage compute costs.

**Input sanitization:** All user inputs to AI services are sanitized to
prevent prompt injection attacks. The sanitization layer strips control
characters, validates input length, and applies content filtering before
passing data to the LLM.

**Output filtering:** AI outputs are validated against a safety filter
that rejects responses containing personally identifiable information
from other tenants, confidential classification markers from
unauthorized documents, or content outside the quality management
domain.

**Model drift monitoring:** The AI platform continuously monitors
prediction accuracy against actual outcomes. When accuracy degrades
below a configurable threshold (default: fifteen percent drop from
baseline), an alert is sent to the AI operations team and the capability
is flagged for model retraining.

**19.7 AI Data Privacy**

**Tenant isolation:** AI models are trained per-tenant. No training data
from one tenant is used to train models for another tenant. The feature
store enforces tenant_id partitioning on all stored features.

**LLM data handling:** When using external LLM providers, the system
sends only the minimum necessary context. No raw quality records are
sent to external LLMs. The RAG pipeline constructs prompts from
anonymized summaries and retrieved document excerpts. The LLM provider
data processing agreement must be signed before activation.

**Data retention:** AI training data, feature store contents, and model
artifacts follow the same retention policies as their source quality
records. When source records are archived or purged, the corresponding
AI data is purged in the next scheduled cleanup.

**Right to explanation:** Users can request an explanation for any AI
suggestion. The explanation includes the source data used, the model
version, the confidence score breakdown, and the feature importance
ranking. This supports regulatory requirements for explainable AI in
quality decisions.

**19.8 AI Usage Policy**

The AI Usage Policy is a controlled document (registered in Document
Control as AI-POL-001) that defines: which quality decisions may be
informed by AI suggestions, which decisions are explicitly excluded from
AI assistance (final product release, safety-critical disposition), the
approval process for enabling new AI capabilities, the review cycle for
AI model performance (quarterly), the escalation path for AI-related
concerns, and the training requirements for users interacting with AI
features. The policy is reviewed annually and updated through the
standard document control change process.

**Chapter 20 --- Offline-First Architecture**

This chapter specifies the offline-first architecture for shop-floor
inspection use cases. Factory environments frequently have unreliable
network connectivity --- Wi-Fi dead zones near metal equipment,
intermittent outages during shift changes, and areas where tablet
connectivity is marginal. The offline architecture ensures that quality
inspectors can continue data entry without interruption and that data
synchronizes reliably when connectivity is restored.

**20.1 Offline Scope**

Offline capability is scoped to the Quality Inspection module\'s data
entry workflow --- the highest-impact use case for shop-floor
reliability. Other modules (NCR, CAPA, Document Control, etc.) require
online connectivity because their workflows involve multi-user
collaboration, approval chains, and real-time notifications that cannot
function meaningfully offline.

  ----------------------------------------------------------------------------
  **Capability**        **Offline     **Rationale**
                        Support**     
  --------------------- ------------- ----------------------------------------
  Inspection data entry Full offline  Primary shop-floor use case; must never
                        support       be blocked by connectivity

  Measurement recording Full offline  Part of inspection workflow; same
                        support       reliability requirement

  Photo attachment      Full offline  Camera capture works offline; photos
  capture               support       queued for upload

  Integrity gate        Cached        Pre-cached validation data enables
  validation            validation    offline gate checks
                        (see 20.4)    

  Inspection submission Queued for    Completed inspections stored locally and
                        sync          synced when online

  NCR creation          Not supported Requires real-time NC number generation
                        offline       and notification

  CAPA actions          Not supported Multi-user workflow requiring
                        offline       server-side coordination

  Dashboard/reporting   Not supported Requires aggregated server-side data
                        offline       
  ----------------------------------------------------------------------------

**20.2 Local Data Storage Strategy**

The tablet client application uses IndexedDB as the primary local data
store, accessed through the Dexie.js library for a structured,
promise-based API. IndexedDB provides sufficient storage capacity
(typically hundreds of megabytes), transactional integrity, and indexed
query support for the inspection workflow.

**20.2.1 Local Database Schema**

  --------------------------------------------------------------------------------
  **Store**               **Purpose**           **Sync           **Retention**
                                                Direction**      
  ----------------------- --------------------- ---------------- -----------------
  pending_inspections     Inspections started   Client to server Until
                          but not yet submitted                  successfully
                                                                 synced, then
                                                                 purged after 7
                                                                 days

  pending_measurements    Measurements recorded Client to server Until parent
                          during offline                         inspection synced
                          inspection                             

  pending_attachments     Photos and evidence   Client to server Until upload
                          captured offline      (binary upload)  confirmed

  cached_checklists       Checklist templates   Server to client Refreshed on each
                          for offline           (read-only)      successful sync;
                          inspection start                       stale after 24
                                                                 hours

  cached_sampling_plans   Sampling plans for    Server to client Refreshed on each
                          offline lot setup     (read-only)      successful sync

  cached_gate_data        Integrity gate        Server to client Refreshed on each
                          validation data (see  (read-only)      successful sync;
                          20.4)                                  stale after 8
                                                                 hours

  sync_queue              Ordered queue of      Client metadata  Cleared on
                          pending sync                           successful sync
                          operations                             

  sync_log                History of sync       Client metadata  Rolling 7-day
                          operations for                         retention
                          troubleshooting                        
  --------------------------------------------------------------------------------

**20.3 Synchronization Architecture**

**20.3.1 Sync Protocol**

Synchronization follows a store-and-forward pattern. When online, the
client syncs continuously (real-time submission). When offline, data
accumulates in the local stores and sync_queue. When connectivity is
restored, the sync engine processes the queue in order.

***Diagram: Synchronization Flow***

sequenceDiagram participant Tablet as Tablet Client participant SW as
Service Worker participant Queue as Sync Queue participant API as Server
API Tablet-\>\>SW: Inspection data saved locally SW-\>\>Queue: Add to
sync queue with sequence number SW-\>\>SW: Check connectivity alt Online
SW-\>\>API: POST /api/v1/sync/inspections (batch) API\--\>\>SW: 200 OK
with server IDs SW-\>\>Queue: Mark items as synced SW-\>\>Tablet: Update
UI with sync confirmation else Offline SW-\>\>Tablet: Show \"Saved
offline - will sync when online\" Note over SW: Connectivity listener
monitors network status SW-\>\>SW: On reconnect: process queue in order
end

**20.3.2 Sync Batch Processing**

The sync engine processes pending items in batches of up to twenty
records per request. Each batch is submitted as an atomic unit ---
either all records in the batch are accepted or the entire batch is
retried. This prevents partial sync states that would be difficult to
reconcile. The server assigns definitive IDs and timestamps upon
receipt, replacing the client-generated temporary identifiers.

**20.4 Offline Authentication**

Offline authentication uses a pre-cached authentication token with an
extended validity period for offline scenarios. When the tablet is
online, the client obtains a standard JWT with a normal expiry (fifteen
minutes) plus an offline token with a longer expiry (configurable,
default eight hours --- matching a single shift). The offline token is
stored encrypted in IndexedDB using the Web Crypto API with a key
derived from the user\'s PIN.

**Offline token scope:** The offline token grants read access to cached
reference data and write access to local inspection stores only. It
cannot be used to authenticate against server API endpoints. When
connectivity is restored, the client must obtain a fresh standard JWT
before syncing queued data.

**Security constraints:** If the offline token expires (shift ends), the
user must re-authenticate when connectivity is restored. Offline tokens
are invalidated server-side if the user\'s account is suspended or their
role permissions change.

**20.5 Offline Integrity Gate Validation**

Integrity gate validation during offline operation uses pre-cached gate
data. When the tablet syncs, it downloads the current gate validation
dataset for the logged-in inspector: their current qualifications and
expiry dates, the calibration status and next-due dates of equipment
assigned to their area, and the current released checklist versions for
products in their inspection scope.

This cached data enables the tablet to perform integrity gate checks
without server connectivity. A staleness indicator shows how old the
cached data is. If the cached data is older than the configurable
staleness threshold (default eight hours), the system warns the
inspector that gate validation may not reflect the latest status and
recommends reconnecting before starting new inspections.

**20.6 Conflict Resolution**

Conflict resolution applies when the same inspection record could
theoretically be modified on multiple devices or when server-side
changes occur during an offline period.

  -----------------------------------------------------------------------
  **Conflict       **Resolution Strategy**      **Rationale**
  Type**                                        
  ---------------- ---------------------------- -------------------------
  Same inspection  Prevented by design:         Conflict avoidance is
  on two devices   inspections are assigned to  preferred over conflict
                   a single inspector and       resolution for quality
                   device pairing               data integrity

  Checklist        Client submits with the      The inspection is valid
  version changed  checklist version that was   against the checklist
  while offline    active at inspection start   that was current when the
                   time; server accepts and     inspector started work
                   records the version used     

  Equipment        Inspection is accepted with  Data is preserved rather
  calibration      a flag indicating post-hoc   than lost; the gap is
  expired during   calibration gap detected;    flagged for human review
  offline period   quality engineer reviews     rather than silently
                   during disposition           discarded

  Inspector        Same as calibration:         Same rationale: preserve
  qualification    accepted with flag, routed   data, flag the integrity
  expired during   for quality engineer review  concern for human
  offline period                                judgment

  Duplicate        Server uses client-generated Exactly-once processing
  submission       idempotency key to           guarantee despite
  (retry after     deduplicate; second          unreliable network
  timeout)         submission returns the       
                   result of the first          
  -----------------------------------------------------------------------

**20.7 Retry Queue and Error Handling**

The sync queue implements an exponential backoff retry strategy for
failed sync attempts. First retry after five seconds, second after
fifteen seconds, third after sixty seconds, fourth after five minutes,
then every fifteen minutes until successful or until the maximum retry
window (twenty-four hours) is exceeded. Records that exceed the retry
window are flagged as sync_failed and require manual intervention by a
supervisor.

The tablet UI displays a persistent sync status indicator showing:
number of pending items, time since last successful sync, any sync
errors with human-readable descriptions, and a manual sync trigger
button. The supervisor dashboard includes a view of all tablets with
pending sync items for fleet-wide monitoring.

**20.8 Recovery Strategy**

In the event of catastrophic local data loss (tablet hardware failure,
factory reset, app uninstall), any inspections that were successfully
synced to the server are safe. Inspections in the local pending queue
that were not yet synced are lost. To mitigate this risk: the tablet app
persists data to IndexedDB immediately upon each measurement entry (not
just on inspection completion); the sync engine attempts to sync partial
inspections (draft status) when connectivity is available, not just
completed inspections; and the supervisor dashboard shows when a
tablet\'s last sync is older than the configurable alert threshold
(default two hours), enabling proactive intervention before data loss
risk grows.

Post-recovery, the inspector re-authenticates online, receives fresh
cached data, and can begin new inspections immediately. Lost offline
inspections that were not synced must be re-entered manually --- the
supervisor is notified of the gap for quality assurance follow-up.

**Chapter 21 --- Reporting and Analytics Engine**

**21.1 Reporting Architecture**

The Reporting Engine is a cross-cutting read-model service that
aggregates data from all domain modules into pre-computed analytics
tables optimized for dashboard queries. It follows the CQRS pattern:
domain modules own the write side (operational data), and the reporting
engine owns the read side (analytical data). Data flows from operational
tables to reporting tables through domain event consumption and
scheduled materialization jobs.

The reporting engine does not query operational tables directly.
Instead, it maintains its own denormalized reporting schema
(reporting.\*) populated through event handlers and nightly batch
reconciliation. This ensures that reporting queries never impact
operational performance and that the reporting data model can evolve
independently of operational schemas.

**21.2 KPI Framework**

  ------------------------------------------------------------------------
  **KPI**         **Data Source **Calculation**        **Refresh
                  Modules**                            Frequency**
  --------------- ------------- ---------------------- -------------------
  First Pass      Inspection    Passed lots / total    Real-time
  Yield (FPY)                   lots inspected, by     (event-driven)
                                line, product, period  

  Defect PPM      Inspection,   Defective parts per    Real-time
                  NC            million                (event-driven)
                                received/produced      

  CAPA On-Time    CAPA          CAPAs closed within    Daily (batch)
  Closure Rate                  target date / total    
                                CAPAs due              

  CAPA            CAPA          CAPAs verified         Daily (batch)
  Effectiveness                 effective / total      
  Rate                          verified CAPAs         

  NC Aging        NC            Average days in each   Daily (batch)
                                NC state; overdue      
                                count                  

  Supplier Score  Supplier      Rolling scorecard      Monthly (batch)
  Trend           Quality       values per supplier    
                                per period             

  Audit Coverage  Audit         Processes audited /    Monthly (batch)
                                total processes in     
                                scope                  

  Training        Training      Qualified employees /  Daily (batch)
  Compliance                    required qualified     
                                employees              

  Calibration     Calibration   Equipment calibrated   Daily (batch)
  On-Time Rate                  before due / total due 

  Cost of Quality NC, CAPA,     Prevention +           Monthly (batch)
                  Inspection,   Appraisal + Internal   
                  Training      Failure + External     
                                Failure costs          
  ------------------------------------------------------------------------

**21.3 Cost of Quality Analysis**

CoQ data is derived from quality activities across all modules.
Prevention Costs include training expenses (from the Training module),
quality planning effort (from inspection plan creation), and supplier
qualification activities. Appraisal Costs include inspection labor (from
inspection time records), calibration costs, audit expenses, and testing
laboratory fees. Internal Failure Costs include scrap and rework costs
(from NC disposition records), reinspection costs, and production delays
attributed to quality holds. External Failure Costs include customer
complaint handling, warranty claims, product recalls, and regulatory
penalties.

**21.4 Dashboard Architecture**

Dashboards are implemented as React components consuming data from
dedicated reporting API endpoints. Each dashboard supports configurable
date ranges, site/line/product filtering, and drill-down from summary to
detail. Dashboard data is cached in Redis with a TTL matching the data
refresh frequency (real-time KPIs: thirty-second cache; daily KPIs:
one-hour cache; monthly KPIs: twelve-hour cache). The Management
Dashboard provides a monthly quality story in five minutes with
drill-down to any underlying detail.

**21.5 Data Retention and Archival**

Reporting data follows a tiered retention strategy. Hot data (current
year plus one year) resides in the primary PostgreSQL instance with full
indexing for real-time dashboard queries. Warm data (two to five years)
uses PostgreSQL partitioned tables with reduced indexing for trend
analysis and periodic reports. Cold data (five to fifteen years,
regulatory dependent) is exported to compressed Parquet files in object
storage for regulatory audit and legal discovery. Data beyond the
retention period is purged per the data governance policy. Nightly
refresh provides sufficient data currency for management dashboards
while maintaining query performance.

**Chapter 22 --- Notification and Communication System**

**22.1 Notification Architecture**

The Notification System is a cross-cutting infrastructure service that
delivers timely, contextual notifications to users and external systems.
It supports multiple delivery channels, user preference management,
escalation workflows, and notification audit trails. Notifications are
critical for ensuring that quality events requiring attention are
surfaced to the appropriate personnel within defined time constraints.

**22.2 Notification Channels**

  ---------------------------------------------------------------------------
  **Channel**    **Use Case**       **Implementation**    **Delivery
                                                          Guarantee**
  -------------- ------------------ --------------------- -------------------
  In-App         All notifications; WebSocket push +      At-least-once;
                 primary channel    in-app notification   persisted in
                                    center                database

  Email          Formal             SMTP adapter with     At-least-once;
                 notifications,     template engine       delivery receipt
                 approvals,                               tracking
                 escalations                              

  SMS (optional) Critical alerts,   SMS gateway adapter   Best-effort;
                 urgent escalations (Twilio/similar)      cost-controlled

  Push           Tablet/mobile      Web Push API (Service Best-effort;
  Notification   alerts for floor   Worker)               offline queuing
                 users                                    
  ---------------------------------------------------------------------------

**22.3 Notification Event Mapping**

  -----------------------------------------------------------------------------
  **Domain Event**        **Recipients**   **Channels**    **Urgency**
  ----------------------- ---------------- --------------- --------------------
  InspectionFailed        Supervisor,      In-App, Email   High
                          Quality Engineer                 

  NonConformanceRaised    Quality Manager, In-App, Email,  Critical
  (Critical)              Plant Manager    SMS             

  CAPAOverdue             CAPA Owner,      In-App, Email   High
                          Quality Manager                  

  CalibrationDue          Calibration      In-App, Email   Medium
                          Owner                            

  CalibrationOverdue      Calibration      In-App, Email   High
                          Owner, Quality                   
                          Manager                          

  QualificationExpiring   Employee,        In-App, Email   Medium
                          Training Manager                 

  DocumentReleased        Distribution     In-App, Email   Normal
                          list members                     

  AuditFindingMajor       Quality Manager, In-App, Email   High
                          Area Owner                       

  SCAROverdue             SQE, Quality     In-App, Email   High
                          Manager                          

  SPCViolation            Operator,        In-App, Push    High
                          Supervisor,                      
                          Engineer                         
  -----------------------------------------------------------------------------

**22.4 User Preference Management**

Users can configure notification preferences per event type and channel.
Preferences include: channel selection (which channels to receive for
each event type), quiet hours (suppress non-critical notifications
outside working hours), digest mode (batch non-urgent notifications into
periodic summaries), and escalation opt-in (receive escalation
notifications for team members). Mandatory notifications (regulatory,
safety-critical) cannot be suppressed by user preferences.

**22.5 Notification Template Engine**

Notification content is generated from templates stored in the database.
Templates support variable substitution from the triggering domain event
payload, conditional sections based on recipient role, multi-language
support through the i18n framework, and channel-specific formatting
(HTML for email, plain text for SMS, structured data for in-app).
Templates are versioned and managed through the admin console.

**Chapter 23 --- Integration and API Gateway**

**23.1 API Design Principles**

All external-facing APIs follow REST conventions with JSON payloads,
consistent error response format, and OpenAPI 3.0 documentation. API
versioning uses URL path prefixing (/api/v1/). Breaking changes
increment the version number; non-breaking additions maintain the
current version. All endpoints enforce authentication (JWT),
authorization (RBAC), rate limiting, and request/response logging.

**23.2 API Gateway Architecture**

The API Gateway is the single entry point for all client requests (web
frontend, tablet app, supplier portal, future ERP integrations). It
handles: TLS termination, JWT validation, rate limiting (configurable
per client type --- internal frontend, supplier portal, external API),
request routing to the appropriate module controller, CORS policy
enforcement, request/response logging with correlation IDs, and API key
management for external integrations.

**23.3 Integration Patterns**

  --------------------------------------------------------------------------
  **Integration**    **Pattern**      **Protocol**       **Status**
  ------------------ ---------------- ------------------ -------------------
  ERP (SAP, Oracle,  Event-driven +   REST API or        Future (Phase 2
  etc.)              scheduled sync   file-based         company vision)
                                      (CSV/XML)          

  Gauge/CMM Data     File import or   CSV import or      Future (post v1)
  Import             direct           instrument SDK     
                     integration                         

  Email Server       SMTP outbound    SMTP/TLS           Included in v1

  Object Storage     Direct           S3-compatible API  Included in v1
  (MinIO)            integration                         

  LLM Provider       REST API         HTTPS with API key Included in v1
                                                         (Phase 2+)

  Supplier Portal    Same API, scoped REST API with      Included in v1
                     access           supplier JWT       

  SSO / Identity     OIDC/SAML        OIDC or SAML 2.0   Optional in v1
  Provider           federation                          
  --------------------------------------------------------------------------

**23.4 Webhook Architecture**

Berex Tech - QMS exposes a configurable webhook system for external
systems to subscribe to quality events. Webhook subscriptions define:
the target URL, the events to subscribe to (from the domain event
catalog), authentication method (HMAC signature, bearer token, or mutual
TLS), retry policy (exponential backoff, maximum three retries), and
payload format (full event payload or summary). Webhook deliveries are
logged and available through the admin console for troubleshooting.

**23.5 API Response Standards**

  ------------------------------------------------------------------------
  **Response    **Standard**                   **Example**
  Element**                                    
  ------------- ------------------------------ ---------------------------
  Success       { data: {\...}, meta: {        200 OK with resource data
  response      requestId, timestamp } }       

  List response { data: \[\...\], meta: {      200 OK with pagination
                total, page, pageSize,         
                requestId } }                  

  Error         { error: { code, message,      400/422 with validation
  response      details, requestId } }         details

  Created       { data: {\...}, meta: {        201 Created with resource
  response      requestId } }                  and Location header

  Accepted      { data: { taskId }, meta: {    202 Accepted for
  (async)       statusUrl, requestId } }       long-running operations
  ------------------------------------------------------------------------

**Chapter 24 --- Security Architecture**

**24.1 Security Design Principles**

Security is designed in layers. No single control is relied upon
exclusively. Defense-in-depth ensures that a failure in one layer is
mitigated by protections at other layers. The security architecture
addresses authentication, authorization, data protection, audit,
infrastructure hardening, and the additional attack surface introduced
by AI capabilities.

**24.2 Authentication Architecture**

**Primary authentication:** Username/password with bcrypt hashing (cost
factor 12). JWT access tokens with fifteen-minute expiry. Refresh tokens
with seven-day expiry, stored encrypted in HttpOnly cookies, with
Redis-backed revocation list.

**Multi-factor authentication:** Optional TOTP-based MFA. Mandatory for
administrator accounts and configurable per role.

**SSO integration:** OIDC and SAML 2.0 adapters for enterprise identity
provider federation. When SSO is configured, local password
authentication is disabled for federated users.

**Session management:** Concurrent session limiting (configurable,
default three). Session invalidation on password change. Idle timeout
(configurable, default thirty minutes for office, sixty minutes for
floor mode).

**24.3 Authorization Architecture**

The RBAC engine implements a hierarchical permission model. Permissions
are grouped into roles. Roles are assigned to users within a tenant
context. Resource-level permissions (specific records, specific areas)
extend role-based access where needed.

  --------------------------------------------------------------------------
  **Role**         **Scope**         **Key Permissions**
  ---------------- ----------------- ---------------------------------------
  System           Platform-wide     Tenant management, system
  Administrator                      configuration, user management

  Quality Manager  Tenant-wide       All quality operations, approval
                                     authority, AI capability management,
                                     report access

  Quality          Department/area   Inspection approval, NC disposition,
  Supervisor                         team workload management

  Quality Engineer Tenant-wide       RCA/CAPA ownership, SPC management, AI
                   (quality data)    interaction

  Quality          Assigned          Inspection execution, defect reporting,
  Inspector        inspection types  measurement recording

  SQE              Supplier quality  Supplier management, SCAR management,
                   scope             scorecard review

  Internal Auditor Audit scope       Audit execution, finding recording,
                                     report generation

  Calibration      Calibration scope Equipment management, calibration
  Technician                         recording, certificate upload

  Training Manager Training scope    Course management, assignment,
                                     qualification management

  Operator (basic) Limited           Defect reporting only; no approval
                                     authority

  Supplier Portal  Own supplier data View own scorecards, respond to own
  User             only              SCARs, upload certificates

  Read-Only Viewer Configurable      Dashboard and report viewing only; no
                   scope             data modification
  --------------------------------------------------------------------------

**24.4 Data Protection**

**Encryption at rest:** Database encryption using PostgreSQL\'s
Transparent Data Encryption or filesystem-level encryption (LUKS).
Object storage encryption using server-side AES-256.

**Encryption in transit:** TLS 1.2 minimum for all connections. HSTS
headers enforced. Certificate pinning for mobile/tablet clients.

**Sensitive field encryption:** Application-level encryption for PII
fields (employee personal data, supplier contact details) using envelope
encryption with tenant-specific keys.

**Export controls:** Data export operations (CSV export, report
generation, bulk download) are logged to the audit trail with the
exporting user, timestamp, record count, and export format. Export
permissions are controlled separately from view permissions.

**24.5 AI Security Controls**

**Permission-filtered RAG:** The RAG retrieval layer enforces RBAC
permissions --- a user can only receive AI responses grounded in data
they have permission to access.

**No state change from AI:** AI service endpoints have read-only
database access. No AI process can modify quality records.

**Prompt injection prevention:** User inputs to AI services are
sanitized through a dedicated input cleaning layer before reaching the
LLM.

**AI channel logging:** All AI interactions are logged in the dedicated
AI audit trail, enabling security review of AI-mediated information
access.

**24.6 Security Testing Requirements**

Annual penetration testing by a qualified third-party firm. Quarterly
automated vulnerability scanning (OWASP ZAP or equivalent). Static
Application Security Testing (SAST) integrated into the CI pipeline.
Dependency vulnerability scanning with automated alerts for critical
CVEs. Security review as a mandatory gate for all pull requests that
modify authentication, authorization, or AI integration code.

**Chapter 25 --- Development Roadmap**

**25.1 Team Assumption**

The roadmap assumes a team of approximately four to six developers (one
lead/architect, two to three full-stack, one frontend-leaning, plus
part-time QA and DevOps), with the Quality Manager as product owner and
two to three pilot users participating per phase. Each phase ends with
real users in production, not a demo.

**25.2 Phase Plan**

  ---------------------------------------------------------------------------------
  **Phase**      **Duration**   **Scope**                    **Exit Criteria**
  -------------- -------------- ---------------------------- ----------------------
  Phase 1:       4 months       Identity and Access, Product IQC inspectors using
  Foundation                    Catalog, Quality Inspection  tablets for real
                                (IQC only), basic NCR,       incoming inspections
                                database foundation, CI/CD   at one line; integrity
                                pipeline, tablet prototype   gates operational;
                                                             basic NCR from failed
                                                             inspections

  Phase 2: Core  3 months       Full NCR lifecycle, CAPA     Full NC-to-CAPA
  Quality                       with effectiveness           workflow operational;
                                verification, Document       documents migrated;
                                Control, IPQC/OQC inspection calibration and
                                types, Calibration           training data seeding
                                foundation, Training         complete
                                foundation, document         
                                migration                    

  Phase 3:       3 months       Audit Management, Supplier   Quality dashboards
  Intelligence                  Quality with scorecards, SPC live; supplier
                                module, Reporting engine,    scorecards publishing;
                                Notification system, AI      SPC charts
                                Phase 1 (document Q&A)       operational; first AI
                                                             capability deployed

  Phase 4: AI    3 months       AI RCA suggestion, anomaly   AI assistant
  and                           detection, inspection        operational with real
  Optimization                  optimization, supplier       data; supplier portal
                                portal, offline-first for    accessible; offline
                                tablets, advanced reporting  inspection capability
                                                             deployed

  Phase 5:       2 months       Performance optimization,    System at production
  Maturity                      security hardening,          scale; all security
                                penetration testing, user    tests passed;
                                training, operational        operational runbooks
                                documentation, full          complete; formal
                                production deployment        handoff to operations
  ---------------------------------------------------------------------------------

**25.3 Phase Dependencies**

Three hidden critical paths must be tracked from day one: master data
cleansing (parts, suppliers, equipment, personnel must be clean before
Phase 1 go-live), document migration (controlled documents must be
inventoried and migrated during Phase 2 --- this is its own workstream
with named owners), and regulatory scope confirmation (the e-signature
and audit model is designed to be hardenable to 21 CFR Part 11, but this
must be confirmed before Phase 1 to determine validation rigor).

**25.4 Phase Exit Criteria**

Each phase has explicit exit criteria that must be met before the next
phase begins. Exit criteria include: all planned features functional and
tested; no critical or high-severity bugs open; pilot users confirming
usability on real production data; performance benchmarks met (page load
under two seconds, inspection entry under the paper baseline time); and
security scan passing with no critical findings.

**25.5 New Foundation Items from v1.1**

The items introduced in v1.1 (calibration and competency data model, lot
genealogy structure, e-signatures, i18n scaffolding, MDM governance) are
design-and-schema work landing in Phase 1\'s foundation layer. Their
full management UIs are staged to later phases. This adds modest Phase 1
scope but prevents expensive later retrofits.

**Chapter 26 --- Testing Strategy**

**26.1 Testing Philosophy**

Testing in Berex Tech - QMS follows a ten-layer approach designed for a
quality management system where data integrity is non-negotiable. Every
quality record must be provably correct, every workflow transition must
be validated, and every integration point must be verified. The testing
strategy is not an afterthought --- it is an integral part of the
architecture.

**26.2 Testing Layers**

  ---------------------------------------------------------------------------------------------
  **Layer**       **Scope**                      **Tool**                **Coverage Target**
  --------------- ------------------------------ ----------------------- ----------------------
  Unit Tests      Domain entities, value         xUnit, Moq              \> 90% of domain logic
                  objects, business rules                                

  Integration     Repository implementations,    TestContainers          \> 80% of data access
  Tests           database queries               (PostgreSQL), xUnit     

  Application     Use case handlers,             xUnit, MediatR test     \> 85% of application
  Service Tests   command/query processing       harness                 services

  API Contract    REST endpoint request/response Pact, xUnit             100% of public API
  Tests           contracts                      WebApplicationFactory   endpoints

  Workflow Tests  State machine transitions,     xUnit with state        100% of transitions
                  guard conditions               machine test harness    and guards

  UI Component    React components, form         Jest, React Testing     \> 80% of interactive
  Tests           validation                     Library                 components

  End-to-End      Critical user journeys         Playwright              Top 20 user journeys
  Tests                                                                  

  Performance     Response time, throughput      k6, Artillery           All
  Tests           under load                                             performance-critical
                                                                         endpoints

  Security Tests  OWASP Top 10,                  OWASP ZAP, custom auth  All security-sensitive
                  authentication/authorization   tests                   endpoints

  Accessibility   WCAG 2.1 AA compliance         axe-core, Lighthouse    All user-facing
  Tests                                                                  screens
  ---------------------------------------------------------------------------------------------

**26.3 Test Data Management**

Test data is managed through a dedicated test data builder library that
creates realistic quality data scenarios. The builder supports: factory
methods for each domain entity with sensible defaults, scenario builders
that compose multi-entity test cases (an inspection that fails, creates
an NC, triggers CAPA), tenant-isolated test data that does not interfere
between parallel test runs, and database seeding scripts for staging
environments with anonymized production-like data.

**26.4 CI Pipeline Test Gates**

The CI pipeline enforces test gates at each stage. Pull request: unit
tests, integration tests, API contract tests, and static analysis must
pass. Merge to main: all PR gates plus workflow tests, UI component
tests, and security scans. Release candidate: all previous gates plus
end-to-end tests, performance tests, and accessibility tests on the
staging environment. No code reaches production without passing all
gates.

**Chapter 27 --- Deployment and Infrastructure**

**27.1 Deployment Topology**

Berex Tech - QMS is deployed as containerized services using Docker and
orchestrated with Docker Compose for initial deployment (with a
documented migration path to Kubernetes when scale demands it). The
deployment topology separates concerns into distinct containers: the
main application server (ASP.NET Core), the background job processor
(Hangfire), the AI microservice (Python/FastAPI), the PostgreSQL
database, the Redis cache, the MinIO object storage, and the reverse
proxy (Nginx or Traefik).

**27.2 Environment Strategy**

  ----------------------------------------------------------------------------
  **Environment**   **Purpose**        **Data**              **Access**
  ----------------- ------------------ --------------------- -----------------
  Development       Developer local    Seeded test data      Individual
                    environment                              developers

  Integration       Automated test     Test-generated data,  CI pipeline only
                    execution          reset per run         

  Staging           Pre-production     Anonymized            QA team, pilot
                    validation         production-like data  users

  Production        Live system        Real production data  All authorized
                                                             users
  ----------------------------------------------------------------------------

**27.3 CI/CD Pipeline**

Trunk-based development with short-lived feature branches. Every merge
to the main branch triggers: build, unit tests, integration tests,
static analysis, container image build, and push to container registry.
Deployment to staging is automatic on successful build. Deployment to
production is one-click with automatic rollback on health check failure.
Database migrations are versioned, forward-only, and tested on staging
before production application. Backward-incompatible migrations require
a two-phase deployment (add new structure, migrate data, remove old
structure).

**27.4 Backup and Disaster Recovery**

  ----------------------------------------------------------------------------
  **Component**    **Backup Strategy**      **RPO**             **RTO**
  ---------------- ------------------------ ------------------- --------------
  PostgreSQL       Continuous WAL           \< 1 hour (WAL) /   \< 4 hours
  Database         archiving + daily full   \< 24 hours (full)  
                   backup                                       

  Object Storage   Nightly incremental      \< 24 hours         \< 8 hours
  (MinIO)          backup to secondary                          
                   location                                     

  Redis Cache      No backup (ephemeral;    N/A                 \< 5 minutes
                   rebuilt from database on (reconstructable)   
                   restart)                                     

  Application      Version-controlled in    Zero (always        \< 30 minutes
  Configuration    Git repository           current in Git)     

  Container Images Stored in container      Zero (always        \< 15 minutes
                   registry with retention  rebuildable from    
                   policy                   source)             
  ----------------------------------------------------------------------------

**27.5 High Availability Considerations**

For v1 deployment, the system runs on a single-server topology with
automated backups and a documented recovery procedure. The architecture
supports horizontal scaling when needed: the application server is
stateless (session data in Redis), the database supports read replicas
for reporting queries, and the background job processor supports
multi-instance execution with distributed locking. Migration to a
multi-server or Kubernetes deployment is a documented operational
change, not an architectural change.

**Chapter 28 --- Monitoring and Logging**

**28.1 Monitoring Architecture**

Monitoring operates at four levels: infrastructure metrics (CPU, memory,
disk, network), application metrics (request rates, response times,
error rates, queue depths), business metrics (inspections submitted per
hour, NC aging, sync queue depth), and health checks (per-module health
endpoints aggregated at the platform level).

**28.2 Metrics Collection**

  ---------------------------------------------------------------------------
  **Metric         **Collection Method** **Storage**     **Alerting**
  Category**                                             
  ---------------- --------------------- --------------- --------------------
  Infrastructure   Prometheus            Prometheus TSDB Grafana alerts
                   node_exporter                         

  Application      Prometheus .NET       Prometheus TSDB Grafana alerts
                   client library                        

  Business         Custom metric         Prometheus TSDB Grafana alerts +
                   emission from domain                  in-app notifications
                   events                                

  Health Checks    ASP.NET Health Checks Prometheus +    PagerDuty/Opsgenie
                   middleware            uptime probe    integration
  ---------------------------------------------------------------------------

**28.3 Structured Logging**

All application logging uses Serilog with structured log entries. Every
log entry includes: timestamp (UTC), log level, message template (not
interpolated string), correlation_id (request-scoped), tenant_id,
user_id (when authenticated), module_name, and structured properties
relevant to the log event. Logs are shipped to a centralized aggregation
platform (Seq for development/staging, ELK stack or equivalent for
production). Log retention follows the same tiered strategy as data
retention.

**28.4 Business Canary Alerts**

Beyond infrastructure monitoring, the system implements business canary
alerts that detect operationally significant conditions. No inspections
submitted during working hours at an active site triggers a canary alert
(possible system adoption issue or outage). Sync queue depth exceeding
threshold triggers a connectivity alert. Zero AI interactions for more
than twenty-four hours triggers an AI service health check. These canary
alerts bridge the gap between infrastructure health (everything is
running) and business health (the system is being used as intended).

**28.5 Support Tiers**

  ----------------------------------------------------------------------------
  **Tier**         **Condition**          **Response   **Action**
                                          Time**       
  ---------------- ---------------------- ------------ -----------------------
  Floor-Stopping   System unusable for    Minutes      Paper-fallback
                   floor inspectors                    activation; immediate
                                                       engineering response

  Degraded         One module             Hours        Engineering
                   unavailable; core                   investigation;
                   functions operational               workaround
                                                       communication

  Cosmetic         UI issue, non-blocking Backlog      Prioritized in next
                   defect                              sprint
  ----------------------------------------------------------------------------

**Chapter 29 --- Risk Analysis**

**29.1 Risk Register**

  -----------------------------------------------------------------------------------------------------
  **\#**   **Risk**         **Type**    **Likelihood**   **Impact**   **Mitigation**
  -------- ---------------- ----------- ---------------- ------------ ---------------------------------
  1        Floor adoption   Adoption    High if ignored  Fatal        Tablet-first design; integrity
           failure ---                                                gates invisible when satisfied;
           inspectors                                                 Phase 1 pilot with real
           revert to paper                                            inspectors; entry-time KPI vs
                                                                      paper baseline

  2        Master data debt Technical   High             High         MDM governance with named owners;
           --- dirty parts,                                           import validators; go-live gate
           suppliers,                                                 on master data quality
           equipment                                                  
           masters                                                    

  3        AI disappoints   Adoption    Medium-High      High         Staged rollout tied to data
           early and                                                  foundations;
           poisons user                                               grounded-with-citations only;
           trust                                                      usefulness metrics with honest
           permanently                                                kill switch

  4        Scope creep      Business    High             High         Scope wall in this blueprint;
           toward                                                     change control via product owner;
           production                                                 expansion only after Phase 3 exit
           before quality                                             criteria
           core is stable                                             

  5        Workflow         Business    Medium           Medium       Workflow parameters as
           rigidity ---                                               configuration;
           system says X,                                             deviation-with-justification
           plant does Y                                               paths; pilot feedback loops

  6        Key-person       Technical   Medium           High         Documentation-as-code; this
           dependency in a                                            blueprint as controlled document;
           small                                                      pairing; no bus-factor-1 modules
           development team                                           

  7        Security breach  Security    Low-Medium       High         Layered controls; annual pen
           or data leak                                               tests; permission-filtered RAG;
           including via AI                                           export logging
           channel                                                    

  8        Wi-Fi dead zones Technical   Medium           High         Phase 1 plant Wi-Fi survey; AP
           break floor                                                remediation budget; offline-first
           usage                                                      architecture (Chapter 20)

  9        Regulatory scope Business    Medium           High         E-signature and audit model
           stricter than                                              hardenable to 21 CFR Part 11;
           assumed                                                    confirm scope before Phase 1

  10       Document         Business    High             Medium       Own Phase 2 workstream with
           migration effort                                           inventory, named owners, and
           underestimated                                             cut-off policy

  11       KPI mistrust     Adoption    Medium           High         Published KPI definitions;
           (dashboard                                                 nightly reconciliation job;
           numbers are                                                parallel-run manual cycle
           wrong)                                                     

  12       LLM vendor       Technical   Medium           Low-Medium   Provider-abstraction layer;
           dependency                                                 on-prem fallback; AI is assistive
           (pricing,                                                  so degradation is inconvenient,
           availability,                                              not fatal
           policy)                                                    

  13       Scope expansion  Project     Medium           Medium       New items are schema and
           from v1.1 added                                            foundation, not full UIs; UIs
           modules inflates                                           staged to later phases; Phase 1
           Phase 1                                                    timebox extended honestly
  -----------------------------------------------------------------------------------------------------

**Chapter 30 --- Reading Guide and Governance**

**30.1 Audience Reading Guide**

**Business owner and top management:** Read Chapters 1, 2, 25, and 29
--- the why, the shape, the plan, and the risks. The value case lives in
Chapter 1\'s measurable targets; hold the project accountable to them.

**Project manager:** Read Chapters 25, 26, and 29 --- phases with exit
criteria, testing gates, and the risk register. The three hidden
critical paths to track from day one: master data cleansing, document
migration, and regulatory scope confirmation.

**Software developers:** Read Chapters 3 through 9 and Chapter 23 ---
architecture decisions with rationale, module specifications written for
independent build, the data model, workflow definitions, and API
conventions.

**Future developers and maintainers:** Read Chapter 2 (why modules
interlock), Chapter 3 (why modular monolith and where extraction seams
are), Chapter 5 (data model philosophy), and Chapter 18 (AI contract
with users). These are the decisions that will look arbitrary in three
years unless the reasoning is preserved.

**UI/UX designers:** Read Chapter 6 --- design principles (especially
the six non-negotiable rules), key screen specifications, and the
two-density model. Floor Mode is the highest-risk design surface.

**Security team:** Read Chapters 24, 19, and 20 --- security
architecture, AI governance, and offline authentication. The AI channel
is an additional attack surface that requires dedicated security review.

**QA team:** Read Chapter 26 --- ten-layer testing strategy with
coverage targets and CI pipeline gates.

**30.2 Document Governance**

This document becomes controlled document ARCH-0001 inside the Berex
Tech - QMS system it describes --- versioned, changes approved by the
Quality Manager plus the technical lead. Until the system exists,
governance follows a manual process: changes are tracked in the Version
History table, and any modification requires sign-off from both the
Quality Manager (business authority) and the Lead Architect (technical
authority).

**30.3 Assumption Register**

Throughout this document, assumptions that must be confirmed before
implementation begins are marked with the symbol. The following
assumptions require explicit confirmation before Phase 1 development
starts:

  ------------------------------------------------------------------------------
  **\#**   **Assumption**           **Impact If Wrong**    **Owner to Confirm**
  -------- ------------------------ ---------------------- ---------------------
  1        Single manufacturing     Affects tenant model   Quality Manager
           site initially,          complexity and Phase 1 
           multi-site later         scope                  

  2        User count: 20-50        Affects infrastructure Quality Manager
           quality team members     sizing and license     
           initially                planning               

  3        No existing ERP          Adds significant Phase Plant Manager
           integration required for 1 scope if ERP         
           Phase 1                  integration is         
                                    mandatory              

  4        Cloud deployment (not    On-prem changes        IT Manager
           on-premises)             deployment             
                                    architecture and LLM   
                                    provider options       

  5        Quality data retention:  Affects archival       Quality Manager /
           minimum 7 years          strategy and storage   Legal
                                    costs                  

  6        Regulatory scope: ISO    Determines validation  Quality Manager
           9001 (not 21 CFR Part    rigor for e-signatures 
           11, AS9100)              and audit trail        

  7        RPO \< 1 hour, RTO \< 4  Stricter targets       Plant Manager
           hours acceptable         require HA             
                                    infrastructure         
                                    investment             

  8        Plant Wi-Fi adequate for Dead zones require AP  IT Manager
           tablet use in production remediation budget or  
           areas                    heavier offline        
                                    capability             
  ------------------------------------------------------------------------------

**30.4 Pre-Implementation Checklist**

Before any code is written, the following steps must be completed:
confirm all assumptions in the register above; appoint the product owner
(Quality Manager recommended) and the master data owner; run the plant
Wi-Fi and tablet survey; sign the LLM provider data processing agreement
(long lead time --- start early even though AI ships in Phase 2);
approve Phase 1 scope and team composition; complete master data
inventory and quality assessment; and then --- and only then ---
implementation begins.

**30.5 Architecture Decision Records**

All significant architecture decisions are documented as Architecture
Decision Records (ADRs) stored in the /docs/adr directory of the source
repository. Each ADR follows a standard template: Title, Status
(Proposed, Accepted, Deprecated, Superseded), Context, Decision,
Consequences, and Related ADRs. ADRs are reviewed during pull requests
that affect system architecture. This practice ensures that future
maintainers can understand not just what was built, but why.

**30.6 Architectural Governance**

The Architecture Review Board (ARB) --- comprising the lead architect,
senior developers, the security lead, and the QA lead --- meets monthly
to review: proposed architectural changes (assessed against this
blueprint for consistency), technical debt register, performance trend
analysis, security posture review, and roadmap alignment. ARB decisions
are recorded as meeting minutes and, where applicable, as ADRs.

**--- End of Berex Tech - QMS Enterprise Software Architecture Blueprint
v1.1 (Frozen Edition) ---**

BEREX-QMS-ARCH-v1.1-FROZEN \| July 2026 \| Confidential

*This document is the single source of truth for the Berex Tech - QMS
engineering team.*
