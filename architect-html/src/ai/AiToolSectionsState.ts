/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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
*/

import { ApiSection } from '@/ai/AiAgentTypes';
import { action, observable } from 'mobx';

const SECTION_OVERRIDES_STORAGE_KEY = 'origam-ai-section-overrides';
const LEGACY_ENABLED_SECTIONS_STORAGE_KEY = 'origam-ai-enabled-sections-v2';

export class AiToolSectionsState {
  @observable accessor sections: ApiSection[] = [];
  @observable accessor sectionsAvailable: boolean = true;
  @observable accessor overrides: Record<string, boolean> = loadSectionOverrides();
  @observable accessor swaggerUrl: string = '';

  private pendingLoad: Promise<void> | null = null;

  get enabledSections(): string[] {
    return this.sections.filter(section => this.isEnabled(section)).map(section => section.name);
  }

  isEnabled(section: ApiSection): boolean {
    return this.overrides[section.name] ?? section.enabledByDefault;
  }

  load(): Promise<void> {
    this.pendingLoad ??= this.fetchSections();
    return this.pendingLoad;
  }

  @action
  setOverrides(overrides: Record<string, boolean>) {
    this.overrides = overrides;
    saveSectionOverrides(overrides);
  }

  private async fetchSections() {
    try {
      const response = await fetch('/agent/architect/sections');
      const payload = await response.json();
      this.applySections(payload.available !== false, payload.sections, payload.baseUrl);
    } catch {
      this.applySections(false, [], null);
    }
  }

  @action
  private applySections(available: boolean, sections: unknown, baseUrl: unknown) {
    this.sectionsAvailable = available;
    this.sections = Array.isArray(sections) ? (sections as ApiSection[]) : [];
    this.swaggerUrl =
      typeof baseUrl === 'string' && baseUrl.length > 0
        ? `${baseUrl.replace(/\/+$/, '')}/swagger/v1/swagger.json`
        : '';
    this.migrateLegacySectionChoices();
  }

  private migrateLegacySectionChoices() {
    const legacyEnabled = readLegacyEnabledSections();
    if (!legacyEnabled || this.sections.length === 0) {
      return;
    }
    const overrides = { ...this.overrides };
    for (const section of this.sections) {
      if (section.name in overrides) {
        continue;
      }
      if (legacyEnabled.includes(section.name) && !section.enabledByDefault) {
        overrides[section.name] = true;
      }
    }
    this.setOverrides(overrides);
    try {
      localStorage.removeItem(LEGACY_ENABLED_SECTIONS_STORAGE_KEY);
    } catch {
      return;
    }
  }
}

function loadSectionOverrides(): Record<string, boolean> {
  try {
    const raw = localStorage.getItem(SECTION_OVERRIDES_STORAGE_KEY);
    if (!raw) {
      return {};
    }
    const parsed = JSON.parse(raw);
    if (typeof parsed !== 'object' || parsed === null || Array.isArray(parsed)) {
      return {};
    }
    return Object.fromEntries(
      Object.entries(parsed).filter(([, value]) => typeof value === 'boolean'),
    ) as Record<string, boolean>;
  } catch {
    return {};
  }
}

function saveSectionOverrides(overrides: Record<string, boolean>) {
  try {
    localStorage.setItem(SECTION_OVERRIDES_STORAGE_KEY, JSON.stringify(overrides));
  } catch {
    return;
  }
}

function readLegacyEnabledSections(): string[] | null {
  try {
    const raw = localStorage.getItem(LEGACY_ENABLED_SECTIONS_STORAGE_KEY);
    if (!raw) {
      return null;
    }
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? parsed.filter(item => typeof item === 'string') : null;
  } catch {
    return null;
  }
}
