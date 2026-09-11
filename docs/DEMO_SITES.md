# The four demo websites

Four complete websites, one per design, for showing the product to a school and for testing
it by hand. Between them they use **every section, every page type and every appearance
option** the console offers.

| | Design | Website | Address |
|---|---|---|---|
| 1 | **Prestige** | St. Aloysius Senior Secondary School, Bengaluru | `/prestige` |
| 2 | **Campus** | Greenfield Residential Campus, Dehradun | `/campus` |
| 3 | **Bulletin** | Delhi Public Academy, New Delhi | `/bulletin` |
| 4 | **Atrium** | Riverstone International School, Pune | `/atrium` |

Each has **30 home sections**, **9 pages**, **25 content records** (staff, news, events,
departments, settings), a menu and SEO settings.

---

## Turning them on

They seed automatically in development. Anywhere else, set:

```
Seed__EnableShowcaseSites=true
```

They are created once. A site whose key already exists is left alone, so anything you change
in the console survives a restart.

## Starting again

To rebuild them after editing, delete the four and restart the application. `scripts/reset-demo-sites.sh`
does this for the local SQLite database:

```bash
scripts/reset-demo-sites.sh
```

---

## Which site shows which setting

No single site can show every option, because most options are a choice between values. They
are spread so that each value appears on at least one of the four.

| Setting | Prestige | Campus | Bulletin | Atrium |
|---|---|---|---|---|
| Logo shape | original | rounded | **square** | rounded |
| Logo height | 64px | 56px | 48px | 72px |
| Page title size | large | medium | **small** | **xlarge** |
| Page title align | **centre** | left | left | **centre** |
| Notice bar style | solid | **gradient** | **dark** | **outline** |
| Notice scrolls | no | yes (28s) | yes (36s) | no |
| Button style | solid | **soft** | **outline** | **gradient** |
| Button shape | **square** | **pill** | rounded | square |
| Button hover | lift | **fill** | **slide** | **glow** |
| Card hover | lift | **zoom** | **tilt** | **glow** |
| Image hover | zoom | **lift** | **tint** | zoom |
| Link hover | underline | **colour** | underline | colour |
| Section animation | rise | zoom | **none** | slide-left |
| Section backdrop | dots | bubbles | grid | shimmer |
| Admissions status | **Open** | **Opening soon** | Open | **Closed** |
| Header contact | yes | yes | yes | **no** |
| Header CTA | Enquire | Book a visit | Admission enquiry | Request the prospectus |
| Hero slideshow | 3 slides | 4 slides | 2 slides | 3 slides |
| Hero controls | yes | yes | **no** | yes |
| Plain hero artwork | no | no | **yes** | no |
| Opening popup | poster only | **off** | **2 posters + enquiry form** | off |
| Social links | all five | 3 | 3 | 3 |

Bold marks the site to look at for that value.

## What is on every one of them

**All 30 home sections**, filled: hero · welcome · about · principal · chairman · director ·
manager · statistics · courses · departments · why choose us · announcements · latest news ·
upcoming events · gallery · video · testimonials · achievements · admission CTA · brochure ·
contact · partners · footer CTA · timings · crest · alumni · staff list · facilities · founder
· downloads.

**All 8 starter page types** — About, Admission, Facilities, Messages, Gallery, Mandatory
Disclosure, Committee, Contact — each with its structured fields filled in, not just a
paragraph. Plus a **ninth page built from the school's own HTML** (`/fee-structure`), so the
custom-HTML switch is on something you can look at.

**Per-section options** are also exercised rather than only the site-wide defaults: several
sections carry their own entrance animation, their own hover and their own backdrop, which is
what a school does when it wants one section to stand apart.

## Suggested order for a client demo

1. `/prestige` — the formal one. Centred hero, heritage palette, a poster popup on arrival.
2. `/campus` — the warm one. Tall banner, facility panels, rounded everything.
3. `/bulletin` — the practical one. Dense, notices first, tables over cards, a two-poster
   popup with an enquiry form attached.
4. `/atrium` — the editorial one. Large type, a full-width photograph wall, generous spacing.

Then open any of them on a phone: all four are built to the same responsive rules, and the
spacing between sections responds to what each section holds.
