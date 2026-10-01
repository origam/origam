#region license
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
#endregion

using Origam.Architect.Server.Models.Requests;

namespace Origam.Architect.Server.Models.Responses.ModelCheck;

public class ModelCheckResultModel
{
    public DateTime? LastRunAt { get; set; }
    public List<ModelRuleError> RuleErrors { get; set; } = [];
    public List<ModelFileErrorSection> FileErrorSections { get; set; } = [];
}

public class ModelRuleError
{
    public SearchResult Item { get; set; }
    public string Message { get; set; }
}

public class ModelFileErrorSection
{
    public string Caption { get; set; }
    public List<ModelFileError> Errors { get; set; } = [];
}

public class ModelFileError
{
    public string Text { get; set; }
    public string Link { get; set; }
}
