# DEVLOG – Idle Trilogy Roadmap

---

## Week 1 – Core loop & UI  
### Scene + wiring  
- [x] feat(core): add Main.tscn scene skeleton (HUD/Actions/Upgrades)  
- [x] feat(ui): add Write Code button + labels  
- [x] feat(core): add timers (passive tick, autosave)  
### Logic + helpers  
- [x] feat(currency): CurrencyManager (click, passive)  
- [x] feat(util): NumberFormatter (K/M/B)  
- [x] feat(save): SaveService v1 (JSON, autosave, on-quit)  
### Polish  
- [x] chore(ui): responsive layout (containers, anchors)  
- [x] chore(style): AppTheme (fonts, button styles)  
### Milestone  
- [x] docs(changelog): week-1 complete  
- [x] chore(release): tag v0.1-core-loop  

---

## Week 2 – Upgrades & Prestige  
### Upgrades → repeatable with limits  
- [x] feat(upgrades): add limit & purchases to model  
- [x] feat(save): persist upgrade purchase counts  
- [x] feat(ui): show count and dynamic cost on upgrade buttons  
- [x] refactor(economy): compute click/income from base+flat+mult  
### Prestige v1  
- [x] feat(prestige): add InvestorCapital and GlobalMult  
- [x] feat(prestige): prestige button + confirm dialog  
- [x] feat(prestige): run reset keeps meta, rebuilds upgrades  
- [x] feat(save): schema v2 incl. InvestorCapital & UpgradeCounts  
- [x] chore(balance): starter costs & growth; prestige threshold  
### Reset progress  
- [ ] feat(settings): reset progress button + confirmation  
- [ ] feat(save): delete user save helper  
### Milestone  
- [ ] docs(changelog): week-2 complete  
- [ ] chore(release): tag v0.3-prestige  

---

## Week 3 – QoL & Mobile build  
### Offline progress  
- [ ] feat(offtime): grant income since last save (cap 8h)  
- [ ] feat(save): store lastSavedUnix for offline calc  
### Settings & UX  
- [ ] feat(settings): sound toggle, autosave toggle, number format  
- [ ] feat(ui): UI scale slider (desktop/mobile)  
### Android build  
- [ ] feat(build): Android export preset + permissions  
- [ ] chore(ui): touch targets & scaling pass  
### Milestone  
- [ ] docs(changelog): week-3 complete  
- [ ] chore(release): tag v0.4-mobile-offline  

---

## Week 4 – Polish & Release  
### Content & feedback  
- [ ] feat(content): +5–10 upgrades, 2–3 unit types  
- [ ] feat(achievements): local achievements (first $1K, first prestige, etc.)  
- [ ] chore(balance): long-idle soft caps  
### Stability  
- [ ] chore(perf): idle CPU usage & GC check  
- [ ] fix(save): guard corrupt/partial saves  
### Release  
- [ ] docs(changelog): v1.0 notes  
- [ ] chore(release): tag v1.0.0 (Linux + Android)  

---

## Week 5 – Bio Lab reskin & multi-currency  
### Theme & reuse  
- [ ] feat(ui): lab theme reskin  
- [ ] refactor(core): enable multi-currency (Biomass/Energy)  
### Systems  
- [ ] feat(actions): Grow Cells (manual)  
- [ ] feat(units): Petri dish, incubator producers  
- [ ] feat(hud): multi-resource HUD  
### Milestone  
- [ ] docs(changelog): week-5 complete  
- [ ] chore(release): tag v2.1-multicurrency  

---

## Week 6 – Traits & Tech tree v1  
### Data + logic  
- [ ] feat(tech): tech graph JSON loader  
- [ ] feat(traits): permanent multipliers system  
- [ ] feat(ui): scrollable tech grid with icons  
### Save  
- [ ] feat(save): persist tech unlocks & traits  
### Milestone  
- [ ] docs(changelog): week-6 complete  
- [ ] chore(release): tag v2.2-tech-tree  

---

## Week 7 – Mutations & Events  
### Randomness  
- [ ] feat(events): low-frequency event system  
- [ ] feat(mutations): pick 1 of 3 on thresholds  
### Tuning  
- [ ] chore(balance): ensure no softlocks; net-fun bias  
### Milestone  
- [ ] docs(changelog): week-7 complete  
- [ ] chore(release): tag v2.3-events-mutations  

---

## Week 8 – Prestige “DNA Memory”  
### Meta  
- [ ] feat(prestige2): Gene Points + cross-run perks  
- [ ] feat(ui): Run Stats page (time, totals)  
### Save/Balance  
- [ ] feat(save): schema bump for gene perks  
- [ ] chore(balance): first sweep with perks  
### Milestone  
- [ ] docs(changelog): week-8 complete  
- [ ] chore(release): tag v2.4-dna-memory  

---

## Week 9 – Juice & Accessibility  
### Feel  
- [ ] feat(vfx): gain animations; sfx set  
- [ ] feat(a11y): contrast, larger buttons, haptics (Android)  
### Recovery  
- [ ] feat(save): export/import (base64)  
### Milestone  
- [ ] docs(changelog): week-9 complete  
- [ ] chore(release): tag v2.5-juice-a11y  

---

## Week 10 – Content & Release  
### Content  
- [ ] feat(content): +10–20 techs, +3 units, rare events  
### Long-idle  
- [ ] chore(balance): diminishing returns/soft caps  
### Release  
- [ ] docs(changelog): v1.0 notes  
- [ ] chore(release): tag v2.0.0 (Bio Lab release)  

---

## Week 11 – Galaxy scaffold  
### Map & resources  
- [ ] feat(ui): planet list/tabs  
- [ ] feat(resources): Credits, Science, Metals, Fuel  
- [ ] feat(actions): Launch Rocket (science)  
- [ ] feat(data): planet JSON (distance, unlocks, outputs)  
### Milestone  
- [ ] docs(changelog): week-11 complete  
- [ ] chore(release): tag v3.1-galaxy-scaffold  

---

## Week 12 – Colonies & Buildings  
### Per-planet  
- [ ] feat(buildings): Mine/Lab/Refinery levels  
- [ ] feat(ui): planet card (outputs, buttons, progress)  
- [ ] refactor(upgrades): global vs per-planet  
### Milestone  
- [ ] docs(changelog): week-12 complete  
- [ ] chore(release): tag v3.2-colonies  

---

## Week 13 – Logistics & Fuel  
### Routes  
- [ ] feat(logistics): assign ships; throughput caps  
- [ ] feat(fuel): propellant use; refinery efficiency  
### Milestone  
- [ ] docs(changelog): week-13 complete  
- [ ] chore(release): tag v3.3-logistics-fuel  

---

## Week 14 – Research & Milestones  
### Depth  
- [ ] feat(research): tree (reuse Bio tech UI)  
- [ ] feat(milestones): badges with minor perks  
- [ ] chore(balance): reach planet 4-5 in ~1h  
### Milestone  
- [ ] docs(changelog): week-14 complete  
- [ ] chore(release): tag v3.4-research  

---

## Week 15 – Prestige “Warp Reset”  
### Meta  
- [ ] feat(prestige3): Dark Matter on warp reset  
- [ ] feat(worldgen): multiple galaxy seeds  
### Milestone  
- [ ] docs(changelog): week-15 complete  
- [ ] chore(release): tag v3.5-warp-reset  

---

## Week 16 – Offline & Stability  
### Offline calc  
- [ ] feat(offtime): multi-planet offline summary  
- [ ] chore(perf): low CPU while idling  
- [ ] fix(save): integrity checks  
### Milestone  
- [ ] docs(changelog): week-16 complete  
- [ ] chore(release): tag v3.6-offline-stability  

---

## Week 17 – UX Depth  
### Management  
- [ ] feat(ui): planet filters (bottlenecked/full/ready)  
- [ ] feat(notify): indicators for important states  
- [ ] chore(audio): consistent sci-fi set  
### Milestone  
- [ ] docs(changelog): week-17 complete  
- [ ] chore(release): tag v3.7-ux-depth  

---

## Week 18 – Content & Release  
### Content  
- [ ] feat(content): late planets, research, prestige meta-perks  
- [ ] chore(balance): late-game scaling  
### Release  
- [ ] docs(changelog): v1.0 notes  
- [ ] chore(release): tag v3.0.0 (Galactic release)  

---

## Optional Weeks 19–20 – Post-Plan  
### IdleKit  
- [ ] refactor(idlekit): extract shared modules as addon  
- [ ] docs(idlekit): setup & usage  
### Store & Marketing  
- [ ] docs(store): itch.io page, screenshots, trailer  
- [ ] chore(release): tag as needed  

---
