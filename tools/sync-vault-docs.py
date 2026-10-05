#!/usr/bin/env python3
"""Mirror the ADRs and specs from the Obsidian vault into docs/.

The vault is the source of truth: never edit docs/adr or docs/specs by hand, change the vault
and run this script again.

Usage: tools/sync-vault-docs.py <path to the vault "Projet" folder>

What it does:
- copies ADR/*.md to docs/adr/ and the two specs to docs/specs/, with kebab-case file names
- rewrites Obsidian [[wikilinks]] into Markdown links GitHub can follow
- rewrites Obsidian callouts (> [!info]) into GitHub alerts (> [!NOTE])
- regenerates docs/README.md, the index
"""

import re
import sys
import unicodedata
from pathlib import Path

ISSUES_URL = "https://github.com/Kiilse/daily-system/issues"

SPECS = {
    "Ecosystem SaaS - Functional Specification": "functional-specification.md",
    "Ecosystem SaaS - Technical Specification": "technical-specification.md",
}

# Vault pages with a public equivalent outside docs/.
EXTERNAL = {"daily-system - Backlog": ISSUES_URL}

# Vault pages with no public equivalent. A list item (or " · " part of one) that only points to
# these pages is dropped; elsewhere the link is removed and the label kept as plain text.
VAULT_ONLY = {"Projets", "daily-system-backlog/README", "trad-bot", "time-manager", "the-hoarding-cat"}

# Obsidian callout type -> GitHub alert type. Anything else falls back to NOTE.
CALLOUTS = {
    "note": "NOTE", "info": "NOTE", "abstract": "NOTE", "summary": "NOTE", "example": "NOTE",
    "tip": "TIP", "hint": "TIP", "success": "TIP",
    "important": "IMPORTANT",
    "warning": "WARNING", "question": "WARNING", "attention": "WARNING",
    "caution": "CAUTION", "danger": "CAUTION", "failure": "CAUTION", "bug": "CAUTION",
}

WIKILINK = re.compile(r"(!?)\[\[([^\]]+?)\]\]")
TICKET = re.compile(r"^(\d{2})-[a-z0-9-]+$")
CALLOUT = re.compile(r"^(>\s*)\[!(\w+)\]([+-]?)[ \t]*(.*)$")


def slugify(text: str) -> str:
    """File name slug: ASCII, lowercase, words joined by hyphens."""
    ascii_text = unicodedata.normalize("NFKD", text).encode("ascii", "ignore").decode()
    return re.sub(r"[^a-z0-9]+", "-", ascii_text.lower()).strip("-")


def github_anchor(heading: str) -> str:
    """Anchor GitHub generates for a heading: lowercase, keep letters, digits, spaces,
    hyphens and underscores, drop everything else (emoji, punctuation), spaces become hyphens."""
    kept = "".join(c for c in heading.lower() if c.isalnum() or c in " -_")
    return kept.replace(" ", "-")


def adr_file_name(stem: str) -> str:
    return slugify(stem) + ".md"


def build_targets(adr_stems):
    """Vault page name -> path relative to docs/."""
    targets = {stem: f"adr/{adr_file_name(stem)}" for stem in adr_stems}
    targets.update({name: f"specs/{file}" for name, file in SPECS.items()})
    return targets


def relative(from_doc: str, to_doc: str) -> str:
    """Relative link between two paths under docs/ (both one folder deep)."""
    from_dir, to_dir = from_doc.split("/")[0], to_doc.split("/")[0]
    return to_doc.split("/")[1] if from_dir == to_dir else f"../{to_doc}"


def parse_wikilink(inner: str):
    """'Page#Heading|label' -> (page, heading, label). Inside a table the | is escaped as \\|."""
    target, _, label = inner.replace("\\|", "|").partition("|")
    page, _, heading = target.partition("#")
    return page, heading, label or (heading if not page else page)


def only_vault_links(fragment: str) -> bool:
    pages = [parse_wikilink(m.group(2))[0] for m in WIKILINK.finditer(fragment)]
    return bool(pages) and all(page in VAULT_ONLY for page in pages)


def drop_vault_only_items(line: str):
    """Drops the parts of a list item that only link to private vault pages.
    Returns None when nothing is left, so the whole line is removed."""
    item = re.match(r"^(\s*- )(.*)$", line)
    if not item or not WIKILINK.search(line):
        return line
    prefix, body = item.groups()
    kept = [part for part in body.split(" · ") if not only_vault_links(part)]
    return f"{prefix}{' · '.join(kept)}" if kept else None


def convert_links(text: str, current: str, targets: dict, unresolved: list) -> str:
    def replace(match):
        embed, inner = match.group(1), match.group(2)
        page, heading, label = parse_wikilink(inner)

        if embed:
            unresolved.append(f"{current}: embed ![[{inner}]] left as text")
            return label
        if not page:
            return f"[{label}](#{github_anchor(heading)})"
        if page in targets:
            link = relative(current, targets[page])
            if heading:
                link += f"#{github_anchor(heading)}"
            return f"[{label}]({link})"
        if page in EXTERNAL:
            return f"[{label}]({EXTERNAL[page]})"
        ticket = TICKET.match(page)
        if ticket:
            return f"[{label}]({ISSUES_URL}/{int(ticket.group(1))})"
        if page not in VAULT_ONLY:
            unresolved.append(f"{current}: [[{inner}]] has no public target, kept as text")
        return label

    out_lines = []
    in_code = False
    for line in text.split("\n"):
        if line.lstrip().startswith("```"):
            in_code = not in_code
        if in_code:
            out_lines.append(line)
            continue
        line = drop_vault_only_items(line)
        if line is not None:
            out_lines.append(WIKILINK.sub(replace, line))
    return "\n".join(out_lines)


def convert_callouts(text: str) -> str:
    """> [!info] Title  ->  > [!NOTE]  +  > **Title** (GitHub alerts take no inline title)."""
    out_lines = []
    for line in text.split("\n"):
        match = CALLOUT.match(line)
        if match:
            prefix, kind, _fold, title = match.groups()
            out_lines.append(f"{prefix}[!{CALLOUTS.get(kind.lower(), 'NOTE')}]")
            if title:
                out_lines.append(f"{prefix}**{title}**")
        else:
            out_lines.append(line)
    return "\n".join(out_lines)


def adr_title(text: str, fallback: str) -> str:
    heading = re.search(r"^# (.+)$", text, re.MULTILINE)
    return heading.group(1).strip() if heading else fallback


def main() -> int:
    if len(sys.argv) != 2:
        print(__doc__.strip().split("\n\n")[2], file=sys.stderr)
        return 2

    vault = Path(sys.argv[1]).expanduser()
    docs = Path(__file__).resolve().parent.parent / "docs"
    adr_sources = sorted((vault / "ADR").glob("ADR-*.md"))
    if not adr_sources:
        print(f"No ADR found in {vault / 'ADR'}", file=sys.stderr)
        return 1

    targets = build_targets([p.stem for p in adr_sources])
    sources = [(p, targets[p.stem]) for p in adr_sources]
    sources += [(vault / f"{name}.md", targets[name]) for name in SPECS]

    # Start from an empty mirror so a renamed or deleted vault file does not linger here.
    for folder in ("adr", "specs"):
        (docs / folder).mkdir(parents=True, exist_ok=True)
        for old in (docs / folder).glob("*.md"):
            old.unlink()

    unresolved = []
    index = []
    for source, dest in sources:
        text = source.read_text(encoding="utf-8")
        text = convert_callouts(convert_links(text, dest, targets, unresolved))
        (docs / dest).write_text(text.rstrip("\n") + "\n", encoding="utf-8")
        if dest.startswith("adr/"):
            index.append(f"- [{adr_title(text, source.stem)}]({dest})")

    readme = [
        "# Documentation",
        "",
        "> [!NOTE]",
        "> Mirror of the Obsidian vault. Do not edit `adr/` or `specs/` by hand: change the vault,",
        "> then run `tools/sync-vault-docs.py <vault>/Projet`.",
        "",
        "## Specifications",
        "",
        f"- [Functional specification](specs/{SPECS['Ecosystem SaaS - Functional Specification']})",
        f"- [Technical specification](specs/{SPECS['Ecosystem SaaS - Technical Specification']})",
        "",
        "## Architecture Decision Records",
        "",
        *index,
        "",
    ]
    (docs / "README.md").write_text("\n".join(readme), encoding="utf-8")

    print(f"Synced {len(sources)} files into {docs}")
    for warning in unresolved:
        print(f"warning: {warning}", file=sys.stderr)
    return 0


if __name__ == "__main__":
    sys.exit(main())
