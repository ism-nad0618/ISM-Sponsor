# Known Limitations and Future Work
## ISM Sponsor Management System

**Version:** 1.0  
**Date:** March 2026  
**Status:** Pilot-Ready with Documented Constraints

---

## Table of Contents

1. [Prototype Scope Context](#prototype-scope-context)
2. [Known Limitations](#known-limitations)
3. [Future Work Backlog](#future-work-backlog)
4. [Pilot-Specific Constraints](#pilot-specific-constraints)
5. [Post-Pilot Enhancement Roadmap](#post-pilot-enhancement-roadmap)

---

## Prototype Scope Context

### What This System Is

The ISM Sponsor Management System is a **pilot-ready prototype** built to:
- Centralize sponsor data and Letter of Guarantee (LoG) coverage rules
- Replace manual spreadsheet-based processes
- Provide real-time coverage evaluation API
- Establish audit trails and approval workflows
- Support operational monitoring and reporting

### What This System Is Not

This system is **NOT**:
- A full production-hardened enterprise solution (DR, HA, advanced monitoring pending)
- A complete replacement for PowerSchool, NetSuite, or OBS (integration is API-based, not UI redesign)
- A predictive analytics or financial forecasting platform (descriptive reporting only)
- A document management system (no PDF/contract storage yet)
- A notification/workflow engine (no automated emails yet)

### Expected Use Case

- **Pilot deployment** for 1-2 school years at ISM
- **User base:** 10-50 concurrent users (Admin, Admissions, Cashier staff)
- **Data volume:** 100-500 sponsors, 500-2000 students, 200-500 LoGs
- **Integration:** API-based coverage evaluation; manual/scheduled sync for now
- **Operations:** Monitored by IT staff using operations dashboard and runbooks

---

## Known Limitations

### 1. Prototype Scope Only

**Limitation:** This is a pilot prototype, not a fully production-hardened system.

**What's Missing:**
- Comprehensive disaster recovery (DR) plan
- High availability (HA) architecture (no load balancing, no failover)
- Advanced monitoring and alerting (Application Insights basic only)
- Automated scaling for high load
- Full security pen testing and hardening
- Comprehensive error handling for all edge cases

**Impact:** System is suitable for pilot and low-to-moderate load, but requires additional hardening before full production rollout at scale.

**Future Work:**
- Implement HA architecture with load balancing
- Establish DR procedures and backup/restore testing
- Deploy Application Insights with custom alerts
- Conduct security pen testing and address findings
- Add comprehensive error handling and resilience patterns

**Target Timeline:** Phase 2 (post-pilot, 3-6 months)

---

### 2. No Predictive Analytics or Financial Forecasting

**Limitation:** System provides descriptive reporting only (what happened), not predictive analytics (what might happen) or financial forecasting.

**What's Missing:**
- Predictive models for sponsor enrollment trends
- Financial forecasting for coverage expenses
- Budget vs. actual analysis
- Trend analysis and projections
- Machine learning-based anomaly detection

**Impact:** Finance team must use external tools (Excel, BI platforms) for forecasting and predictive analysis.

**Future Work:**
- Integrate with Power BI or Tableau for advanced analytics
- Add predictive models for enrollment and coverage trends
- Build budget vs. actual reporting
- Implement ML-based anomaly detection for unusual coverage patterns

**Target Timeline:** Phase 3 (12-18 months post-pilot)

---

### 3. No Large UI Redesign of PowerSchool, NetSuite, or OBS

**Limitation:** The ISM Sponsor system does not redesign or replace the UIs of external systems. Integration is API-based only.

**What's Missing:**
- Unified UI combining sponsor data, PowerSchool student data, and NetSuite invoicing
- Single-pane-of-glass interface
- Custom widgets or extensions for PowerSchool/NetSuite
- Real-time sync indicators in external system UIs

**Impact:** Users must still navigate to PowerSchool, NetSuite, and OBS for their respective functions. ISM Sponsor system is a separate application.

**Future Work:**
- Investigate embedding ISM Sponsor widgets into PowerSchool (via PowerSchool plugins)
- Explore NetSuite SuiteApp development for integrated sponsor view
- Build unified dashboard pulling data from all systems (future phase)

**Target Timeline:** Phase 4 (18-24 months post-pilot, dependent on vendor APIs)

---

### 4. API or File-Exchange Constraints

**Limitation:** External system integration depends on API availability or file-exchange capabilities. Full real-time sync may not be feasible for all data sets.

**What's Missing:**
- Real-time bidirectional sync with PowerSchool (currently stub/manual)
- Real-time bidirectional sync with NetSuite (currently stub/manual)
- Automated OBS integration (not yet implemented)
- Webhook-based event-driven sync

**Impact:** Some data (students, invoices) may need manual sync or scheduled batch jobs until full API integration is complete.

**Future Work:**
- Complete PowerSchool API integration (student tagging, sponsor linkage)
- Complete NetSuite API integration (invoice creation, payment tracking)
- Implement scheduled sync jobs (nightly or hourly)
- Add webhook listeners for real-time PowerSchool/NetSuite events

**Target Timeline:** Phase 2 (post-pilot, 3-6 months) - Requires API documentation and credentials from PowerSchool/NetSuite

---

### 5. Scheduled Sync May Still Be Needed

**Limitation:** Even with API integration, some data sets may require scheduled batch sync rather than real-time sync due to performance or API rate limits.

**What's Missing:**
- Real-time sync for all entities (sponsor, student, LoG, invoice)
- Event-driven sync architecture
- Conflict resolution for simultaneous edits in multiple systems

**Impact:** Some data may be slightly stale (last sync timestamp shown). Users must understand sync frequency.

**Future Work:**
- Implement scheduled sync jobs (e.g., nightly at 2 AM)
- Add manual "Sync Now" buttons for on-demand sync
- Display last sync timestamp and "next sync at" indicators
- Build conflict resolution UI for manual resolution

**Target Timeline:** Phase 2 (post-pilot, 3-6 months)

---

### 6. Historical Retroactive Restatement Out of Scope

**Limitation:** System is forward-looking. Retroactive restatement of historical data (e.g., recalculating coverage for past school years) is not supported.

**What's Missing:**
- Bulk recalculation of past coverage evaluations
- Historical data correction workflows
- Version control for LoG rules (historicalchanges)

**Impact:** If LoG rules change mid-year or retrospectively, past evaluations are not automatically recalculated. Manual adjustments may be needed.

**Future Work:**
- Add "Recalculate Coverage" feature for date ranges
- Implement LoG rule versioning (effective dates)
- Build historical reconciliation reports

**Target Timeline:** Phase 3 (12-18 months post-pilot)

---

### 7. Advanced Monitoring, DR, and HA Need Further Work

**Limitation:** Basic monitoring via operations dashboard only. Advanced monitoring, disaster recovery, and high availability not yet implemented.

**What's Missing:**
- Real-time application performance monitoring (APM)
- Automated alerting (email/SMS for critical errors)
- Failover and redundancy
- Automated backups with tested restore procedures
- DR runbook with RTO/RPO targets

**Impact:** IT staff must manually monitor operations dashboard. No automatic alerts for downtime or errors. Single point of failure if hosting server fails.

**Future Work:**
- Deploy Application Insights with custom alerts (5xx errors, high response time, sync failures)
- Implement automated database backups (daily full, hourly incremental)
- Test restore procedures (quarterly)
- Deploy to multiple availability zones or regions (Azure)
- Document DR runbook with RTO < 4 hours, RPO < 1 hour

**Target Timeline:** Phase 2 (post-pilot, priority for production rollout)

---

### 8. Sponsor Self-Service Not Implemented

**Limitation:** Sponsor role is a placeholder. Sponsors cannot log in and view their own profiles, students, or LoGs.

**What's Missing:**
- Sponsor self-service portal
- Sponsor user registration and authentication
- Sponsor view of own profile, students, LoGs, invoices
- Sponsor-initiated change requests
- Sponsor document upload (contracts, invoices)

**Impact:** Sponsors must contact Admissions or Cashier for information. No self-service reduces operational efficiency.

**Future Work:**
- Build sponsor self-service portal
- Allow sponsors to view own profile (read-only)
- Allow sponsors to view linked students and LoGs
- Allow sponsors to submit change requests
- Add document upload feature for sponsors

**Target Timeline:** Phase 2 (post-pilot, 6-12 months)

---

### 9. No Automated Email Notifications

**Limitation:** System does not send automated email notifications for change requests, LoG activations, or other events.

**What's Missing:**
- Email notification to requester when change request approved/rejected
- Email notification to Admin when new change request submitted
- Email notification to Admissions when LoG activated
- Email notification for sync failures
- Email notification for data consistency warnings

**Impact:** Users must manually check dashboard or system for updates. Admins may miss pending requests.

**Future Work:**
- Integrate email service (SendGrid, SMTP)
- Build notification templates
- Add user preferences for notification opt-in/opt-out
- Implement notification queue (async)

**Target Timeline:** Phase 2 (post-pilot, 3-6 months)

---

### 10. No Document Attachment Management

**Limitation:** System does not store or manage document attachments (LoG PDFs, contracts, invoices, supporting documents).

**What's Missing:**
- File upload for LoG PDFs
- File upload for sponsor contracts
- File upload for supporting documents (change request evidence)
- Document versioning
- Document access control (who can view which documents)

**Impact:** Documents must be stored in separate file system (SharePoint, Google Drive) and referenced manually.

**Future Work:**
- Add file upload feature to sponsors, LoGs, change requests
- Store files in Azure Blob Storage or local file system
- Add document viewer in UI
- Implement document versioning and audit trail
- Add role-based document access control

**Target Timeline:** Phase 2 (post-pilot, 6-12 months)

---

### 11. Single Sponsor Per Student

**Limitation:** System currently supports one primary sponsor per student. Multi-sponsor scenarios (e.g., divorced parents with separate sponsors) not supported.

**What's Missing:**
- Multiple sponsors per student
- Priority/percentage rules for multi-sponsor coverage
- Split billing across multiple sponsors
- Conflict resolution for overlapping LoGs

**Impact:** Students with multiple sponsors must be handled manually or by choosing one primary sponsor.

**Future Work:**
- Add many-to-many relationship between students and sponsors
- Implement priority rules (primary, secondary sponsors)
- Implement percentage-based split for multi-sponsor coverage
- Build conflict resolution workflow

**Target Timeline:** Phase 3 (12-18 months post-pilot)

---

### 12. Limited Report Customization

**Limitation:** Reports are fixed. Users cannot create custom ad-hoc reports or modify existing report layouts.

**What's Missing:**
- Custom report builder
- Ad-hoc query interface
- Report scheduling (automated generation and email)
- Report subscriptions
- Custom dashboard widgets

**Impact:** Users must request new reports from development team. Cannot answer ad-hoc questions quickly.

**Future Work:**
- Integrate with Power BI or SSRS for custom reporting
- Build query builder UI for ad-hoc reports
- Add report scheduling and subscriptions
- Allow users to customize dashboard widgets
- Add report library with community-shared reports

**Target Timeline:** Phase 3 (12-18 months post-pilot)

---

### 13. No Advanced Analytics or Dashboards

**Limitation:** Dashboards are basic operational views. No advanced analytics, visualizations, or KPIs.

**What's Missing:**
- Interactive charts and graphs
- KPI dashboards (coverage rate, sync success rate, task completion rate)
- Drill-down analytics
- Real-time data refresh
- Comparative analytics (year-over-year, budget vs. actual)

**Impact:** Users mustexport data to Excel or BI tools for advanced analysis.

**Future Work:**
- Integrate with Power BI or Tableau
- Build interactive dashboards with Chart.js or D3.js
- Add real-time data refresh (SignalR)
- Build KPI dashboard for leadership

**Target Timeline:** Phase 3 (12-18 months post-pilot)

---

### 14. Mobile UI Not Optimized

**Limitation:** UI is desktop-focused. Mobile/tablet experience is functional but not optimized.

**What's Missing:**
- Responsive design for mobile (Bootstrap responsive classes used but not tested deeply)
- Mobile-specific navigation (hamburger menu, touch-friendly controls)
- Mobile app (iOS, Android native apps)

**Impact:** Users on mobile devices will have suboptimal experience (small text, difficult navigation).

**Future Work:**
- Test and optimize responsive design for mobile browsers
- Build mobile-specific navigation and layouts
- Consider Progressive Web App (PWA) for offline support
- Evaluate native mobile app development

**Target Timeline:** Phase 4 (18-24 months post-pilot, based on pilot feedback)

---

### 15. Backup and Restore Not Fully Tested

**Limitation:** Backup procedures exist (SQL Server backups) but restore procedures not fully tested.

**What's Missing:**
- Regularly tested restore procedures (monthly tests)
- Documented RTO/RPO targets
- Disaster recovery runbook
- Backup integrity checks

**Impact:** Risk of data loss if backup fails or restore untested.

**Future Work:**
- Implement automated daily backups (full + incremental)
- Test restore procedures monthly
- Document DR runbook with step-by-step restore
- Set RTO < 4 hours, RPO < 1 hour

**Target Timeline:** Phase 2 (post-pilot, priority before production rollout)

---

## Future Work Backlog

### Priority 1: Critical for Production Rollout (Post-Pilot)

1. **Automated PowerSchool/NetSuite Sync**
   - Replace stub implementations with real API integration
   - Implement scheduled sync jobs (nightly)
   - Add conflict resolution workflows
   - **Effort:** 4-6 weeks

2. **Email Notifications**
   - Integrate email service (SendGrid)
   - Build notification templates (change request decisions, LoG activations)
   - Add user notification preferences
   - **Effort:** 2-3 weeks

3. **Disaster Recovery and Backups**
   - Implement automated backups (daily full, hourly incremental)
   - Test restore procedures monthly
   - Document DR runbook
   - **Effort:** 2 weeks

4. **High Availability Architecture**
   - Deploy to multiple availability zones
   - Implement load balancing
   - Add health checks and auto-failover
   - **Effort:** 3-4 weeks

5. **Advanced Monitoring and Alerting**
   - Deploy Application Insights with custom alerts
   - Add real-time error notifications (email/SMS)
   - Build ops dashboard enhancements
   - **Effort:** 2 weeks

---

### Priority 2: Important for User Experience (6-12 Months)

6. **Sponsor Self-Service Portal**
   - Build sponsor authentication and registration
   - Allow sponsors to view own profile, students, LoGs
   - Allow sponsors to submit change requests
   - **Effort:** 6-8 weeks

7. **Document Attachment Management**
   - Add file upload for LoGs, sponsors, change requests
   - Store files in Azure Blob Storage
   - Add document viewer and version control
   - **Effort:** 4-5 weeks

8. **Mobile UI Optimization**
   - Test and optimize responsive design
   - Build mobile-specific navigation
   - Consider PWA for offline support
   - **Effort:** 3-4 weeks

9. **Report Scheduling and Subscriptions**
   - Allow users to schedule reports (daily, weekly, monthly)
   - Email reports to subscribers
   - **Effort:** 2-3 weeks

10. **UX Refinements Based on Pilot Feedback**
    - Address usability issues identified during pilot
    - Simplify complex workflows
    - Improve error messages and help text
    - **Effort:** 2-4 weeks (ongoing)

---

### Priority 3: Nice-to-Have Enhancements (12-18 Months)

11. **Multi-Sponsor Support**
    - Allow multiple sponsors per student
    - Implement priority and percentage rules
    - Build conflict resolution
    - **Effort:** 6-8 weeks

12. **Custom Report Builder**
    - Build ad-hoc query UI
    - Integrate with Power BI or SSRS
    - Allow users to save and share custom reports
    - **Effort:** 8-10 weeks

13. **Advanced Analytics and KPI Dashboards**
    - Build interactive dashboards with charts
    - Add KPIs (coverage rate, task completion, sync success)
    - Real-time data refresh (SignalR)
    - **Effort:** 6-8 weeks

14. **Historical Data Recalculation**
    - Add "Recalculate Coverage" feature for date ranges
    - Implement LoG rule versioning with effective dates
    - Build historical reconciliation reports
    - **Effort:** 4-5 weeks

15. **Predictive Analytics and Forecasting**
    - Integrate ML models for enrollment trends
    - Build financial forecasting reports
    - Add anomaly detection
    - **Effort:** 10-12 weeks (requires data science expertise)

---

### Priority 4: Future Integrations (18-24 Months)

16. **Unified Dashboard with PowerSchool/NetSuite Data**
    - Pull student data from PowerSchool into ISM dashboard
    - Pull invoice data from NetSuite into ISM dashboard
    - Build single-pane-of-glass view
    - **Effort:** 8-10 weeks

17. **PowerSchool Plugin Development**
    - Build PowerSchool plugin to show sponsor/LoG info in student record
    - Embed ISM Sponsor coverage widget in PowerSchool UI
    - **Effort:** 6-8 weeks (requires PowerSchool plugin dev expertise)

18. **NetSuite SuiteApp Development**
    - Build NetSuite SuiteApp for integrated sponsor view
    - Pull LoG data into NetSuite invoice workflow
    - **Effort:** 6-8 weeks (requires NetSuite SuiteApp dev expertise)

19. **OBS Integration (Online Billing System)**
    - Integrate with ISM's OBS for real-time charge evaluation
    - Embed coverage API in OBS charge entry workflow
    - **Effort:** 4-6 weeks (requires OBS API documentation)

20. **Webhook-Based Event-Driven Sync**
    - Replace scheduled  sync with real-time webhooks
    - Listen for PowerSchool/NetSuite events (student added, invoice created)
    - Trigger sync immediately
    - **Effort:** 4-5 weeks

---

## Pilot-Specific Constraints

### Data Volume

**Pilot Constraint:** System tested with 6 demo sponsors, 8 students, 5 LoGs. Not tested at scale.

**Production Expectation:** 100-500 sponsors, 500-2000 students, 200-500 LoGs.

**Risk:** Performance may degrade with larger data volumes.

**Mitigation:**
- Monitor performance during pilot
- Add database indexes if queries slow down
- Implement pagination for large lists (already implemented for 50+ records)
- Plan for performance tuning based on pilot metrics

---

### Concurrent Users

**Pilot Constraint:** System not load-tested for high concurrency.

**Production Expectation:** 10-50 concurrent users.

**Risk:** System may slow down or become unresponsive with many simultaneous users.

**Mitigation:**
- Monitor concurrent user count during pilot
- Implement caching for frequently accessed data
- Add load balancing if needed (Phase 2)

---

### External System Availability

**Pilot Constraint:** PowerSchool and NetSuite sync are stub implementations. Real API behavior unknown.

**Production Expectation:** PowerSchool and NetSuite APIs may have rate limits, downtime, or breaking changes.

**Risk:** Sync failures due to external system issues.

**Mitigation:**
- Implement retry logic with exponential backoff
- Log all sync failures for investigation
- Display sync status prominently in operations dashboard
- Have runbook for sync failure response

---

## Post-Pilot Enhancement Roadmap

### Phase 1: Pilot (Current - 3 Months)
- Deploy pilot to ISM staging environment
- Conduct UAT with real users
- Collect feedback via in-app feedback system
- Monitor operations dashboard for issues
- Address critical defects immediately
- Document lessons learned

### Phase 2: Post-Pilot Hardening (3-6 Months)
- Implement automated PowerSchool/NetSuite sync
- Add email notifications
- Implement DR/HA architecture
- Deploy advanced monitoring and alerting
- Address pilot feedback
- Prepare for production rollout

### Phase 3: Production Rollout (6-12 Months)
- Deploy to production environment
- Migrate real sponsor data from legacy systems
- Train all staff (Admin, Admissions, Cashier)
- Run parallel with legacy systems for 1 month
- Full cutover to ISM Sponsor system
- Continue monitoring and refinement

### Phase 4: Enhancements (12-24 Months)
- Implement sponsor self-service portal
- Add document attachment management
- Build custom report builder
- Integrate advanced analytics
- Implement multi-sponsor support
- Explore PowerSchool/NetSuite plugin development

---

## Success Metrics

### Pilot Success Criteria

- Task completion rate ≥ 90% (users can complete core workflows without assistance)
- No blocking defects (Critical/High severity defects resolved)
- Real-time coverage evaluation API responds in <2 seconds
- No unauthorized access violations
- Positive feedback from pilot users (≥ 70% satisfied)

### Production Success Criteria (Post-Pilot)

- User adoption rate ≥ 80% (staff actively using system)
- Reduction in manual spreadsheet usage (≥ 50% reduction)
- Reduction in billing errors (≥ 30% reduction in sponsor billing disputes)
- Sync success rate ≥ 95% (PowerSchool/NetSuite)
- System uptime ≥ 99% (excluding planned maintenance)
- User satisfaction ≥ 75% (based on feedback surveys)

---

## Continuous Improvement

The ISM Sponsor system is designed for continuous improvement based on pilot feedback and evolving business needs. The roadmap above is flexible and will be adjusted based on:

- Pilot user feedback
- Defect trends and patterns
- Business priority changes
- External system API changes
- Budget and resource availability

**Feedback Channels:**
- In-app feedback system (all users)
- UAT feedback (pilot testers)
- Stakeholder meetings (monthly)
- Operations dashboard metrics (IT monitoring)

---

**End of Known Limitations and Future Work**

---

**Document Version Control:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | March 2026 | [Your Name] | Initial limitations and future work documentation |
