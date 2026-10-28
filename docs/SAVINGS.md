# Documented savings scenarios

Routing flags identify opportunities, not financial benefits. No default dollar
amount, labor rate, schedule, recoverable time or yield target is monetized.
Existing callers without financial inputs receive null (unquantified), never zero.
The legacy EstimatedAnnualSavings field is now nullable and contains only the
projected annual scenario amount. Read Savings for its classification and evidence.

In the Windows application, load a routing, export the savings input template,
complete the JSON and load it using “Load documented savings inputs”. Keep the JSON
as the audit record. Reloading a routing clears its financial scenarios; export a
fresh template because material IDs belong to the current import session.
Delete unused entries. Use exact operation sequence and waste type from the template.
Currency must be an explicit uppercase three-letter code. All money inputs must
use that currency; there is no exchange-rate conversion or aggregate total.

Each entry selects one Basis (enum string) and documents EvidenceReference:

| Basis | Required inputs | Annual formula |
| --- | --- | --- |
| LaborCapacity | ProductionDaysPerYear, EventsPerDay, MeasuredSecondsReducedPerEvent, AffectedOperators, LoadedLaborRatePerHour | seconds / 3600 × events/day × days/year × operators × rate |
| Scrap | ProductionDaysPerYear, UnitsStartedPerDay, BaselineYieldPercent, ImprovedYieldPercent, AvoidableScrapCostPerUnit | starts/day × yield percentage-point improvement / 100 × cost × days/year |
| ThroughputContribution | ProductionDaysPerYear, AdditionalSaleableUnitsPerDay, DemandBackedAdditionalUnitsPerDay, ContributionMarginPerUnit | min(additional units, demand-backed units) × margin × days/year |

For example, 900 measured seconds saved per changeover, two changeovers per day,
200 production days, two affected operators and a documented 40 USD/hour loaded
rate produce 8,000 USD/year of released labor capacity. That is not cash savings.
Use cycle occurrences for cycle work and changeover occurrences for setup work.
Never substitute the entire setup duration, takt gap or passive queue time for
measured recoverable labor. Queue reductions alone do not justify labor savings.

Scrap cost must be avoidable cost net of recovery, not selling price. Yield inputs
must represent actual scrap disposition on a comparable starts basis; first-pass
yield alone may include rework and must not automatically be treated as scrap.
Throughput units must be incremental saleable output supported by bottleneck,
downstream capacity and customer-demand evidence. Margin is incremental contribution,
not revenue. EvidenceReference should identify time studies, measurement dates,
production schedule, cost records, assumptions and any exclusions. Inputs describe
one scenario only; unused fields do not contribute to its formula.

Scenarios can overlap across operations and waste categories. Do not sum them.
Select a non-overlapping financial case and reconcile it outside the opportunity
list. Labor capacity value and throughput contribution are labeled separately
from projected avoidable scrap cost. No financial result is automatically verified.

Optional Realized requires Amount (nonnegative net benefit), PeriodStart,
PeriodEnd (ISO dates, closed before today UTC), VerifiedBy and EvidenceReference.
Only enter financially reconciled actual benefit with supporting measurement,
implementation costs, volume/mix normalization and approval records. It is shown
as a verified realized period total, separately from the projection; never annualized.
This records a verifier's assertion, not an automated audit or access-controlled approval.
Negative net outcomes should be tracked in project accounting; this savings-only
model rejects negative amounts rather than disguising losses as savings.

Missing formula inputs yield an unquantified result with the selected formula.
Explicit zero is valid for quantities, rates and improvements; days must be
positive and at most 366, affected operators positive, yields 0–100 without a
negative improvement. Duplicate scenarios, unknown properties in desktop JSON,
unmatched material/operation/waste and overflow are rejected. Decimal arithmetic
rounds only the final projection to two decimals, midpoint away from zero.
