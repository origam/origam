#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using Origam.Composer.Interfaces.BuilderTasks;

namespace Origam.Composer.Interfaces.Services;

public interface IVisualService
{
    void PrintHeader(string title);

    void PrintProjectValues(
        string name,
        string folder,
        string dockerImageLinux,
        string dockerImageWindows,
        string adminName,
        string adminEmail
    );

    void PrintDatabaseValues(string type, string host, int port, string name, string username);

    void PrintArchitectValues(string dockerImageLinux, string dockerImageWindows, int port);

    void PrintGitValues(bool isEnabled, string user, string email);

    void PrintProjectCreateTasks(List<IBuilderTask> tasks);

    void PrintBye();
}
