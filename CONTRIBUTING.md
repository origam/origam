# How to contribute to Origam
First join [community.origam.com](https://community.origam.com). It is as place where you can report bugs or post ideas regarding new features.

## Submitting changes
If you wish to contribute to the code, the pull requests should be made for the `master` branch. The pull requests for other branches (previous versions) are not desired except of critical bugs related to security or data integrity. The severity of the bug and its acceptability can be discussed on [community.origam.com](https://community.origam.com).

The pull request title needs to contain a prefix describing the general intent of the pull request. The accepted prefixes are following:

- FIX: - a change that fixed a bug
- SECURITY: - a change that fixes a security problem
- FEATURE: - a change that adds a new feature to Origam
- BUILD: - a change related to CI/CD
- UX: - a change that improves user experience with Origam, but it is not considered as a feature
- DEV: - a change that modifies Origam internals, without falling into one of the above categories

The pull request should be linked to the topic on the [community.origam.com](https://community.origam.com). It is done by posting the link to the pull request in a reply to the topic on the [community.origam.com](https://community.origam.com). `origambot` will do the rest.
If the contribution is a new feature, there should be an article on [community.origam.com](https://community.origam.com) documenting this new feature.

The individual commits from the branch don't need to have a prefix describing the intent of the commit. They're being squashed when the pull request is approved.

Commit message template:

```
Capitalized, short (50 chars or less) summary

More detailed explanatory text, if necessary.  Wrap it to about 72
characters or so.  In some contexts, the first line is treated as the
subject of an email and the rest of the text as the body.  The blank
line separating the summary from the body is critical (unless you omit
the body entirely); tools like rebase can get confused if you run the
two together.

Write your commit message in the imperative: "Fix bug" and not "Fixed bug"
or "Fixes bug."  This convention matches up with commit messages generated
by commands like git merge and git revert.

Further paragraphs come after blank lines.

- Bullet points are okay, too

- Typically a hyphen or asterisk is used for the bullet, followed by a
  single space, with blank lines in between, but conventions vary here

- Use a hanging indent
```

There are automated code checks and tests for each pull request. If they don't pass the pull request can't be reviewed.

## Coding conventions

The coding conventions are W.I.P. and will be published later. At the moment the new code should follow the convention of the class/file it is contributed to. The comments should be used only to explain the motivation for the used solution when it is not clear.
The new files should contain licensing information:
```
Copyright 2005 - 2022 Advantage Solutions, s. r. o.
This file is part of ORIGAM (http://www.origam.org).
ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
```
