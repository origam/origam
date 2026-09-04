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
import { saveCustomInstructions } from '@/ai/AiPromptApi';
import { AiToolSectionsState } from '@/ai/AiToolSectionsState';
import { ITabState } from '@/components/editorTabView/ITabState';
import { T } from '@/main';
import { observable } from 'mobx';

export default class AiSettingsModuleState implements ITabState {
  @observable accessor customInstructions: string;
  @observable accessor isActive = false;
  @observable accessor isDirty = false;
  @observable accessor isSaving = false;
  @observable accessor sectionOverrides: Record<string, boolean> | null = null;
  @observable accessor expandedSectionNames: string[] = [];

  label = T('AI settings', 'ai_settings_tab_label');

  constructor(
    public tabId: string,
    customInstructions: string,
    public model: string,
    public router: string,
    public hasApiKey: boolean,
    private toolSectionsState: AiToolSectionsState,
  ) {
    this.customInstructions = customInstructions;
  }

  get sections(): ApiSection[] {
    return this.toolSectionsState.sections;
  }

  get sectionsAvailable(): boolean {
    return this.toolSectionsState.sectionsAvailable;
  }

  get swaggerUrl(): string {
    return this.toolSectionsState.swaggerUrl;
  }

  get enabledSectionCount(): number {
    return this.sections.filter(section => this.isSectionEnabled(section)).length;
  }

  isSectionEnabled(section: ApiSection): boolean {
    const overrides = this.sectionOverrides ?? this.toolSectionsState.overrides;
    return overrides[section.name] ?? section.enabledByDefault;
  }

  isSectionExpanded(section: ApiSection): boolean {
    return this.expandedSectionNames.includes(section.name);
  }

  toggleSectionExpanded(section: ApiSection) {
    this.expandedSectionNames = this.isSectionExpanded(section)
      ? this.expandedSectionNames.filter(name => name !== section.name)
      : [...this.expandedSectionNames, section.name];
  }

  setCustomInstructions(customInstructions: string) {
    this.customInstructions = customInstructions;
    this.isDirty = true;
  }

  toggleSection(section: ApiSection) {
    const overrides = this.sectionOverrides ?? this.toolSectionsState.overrides;
    this.sectionOverrides = { ...overrides, [section.name]: !this.isSectionEnabled(section) };
    this.isDirty = true;
  }

  resetSectionsToDefaults() {
    this.sectionOverrides = {};
    this.isDirty = true;
  }

  *save(): Generator<Promise<any>, void, any> {
    this.isSaving = true;
    try {
      yield saveCustomInstructions(this.customInstructions);
      if (this.sectionOverrides) {
        this.toolSectionsState.setOverrides(this.sectionOverrides);
        this.sectionOverrides = null;
      }
      this.isDirty = false;
    } finally {
      this.isSaving = false;
    }
  }
}
