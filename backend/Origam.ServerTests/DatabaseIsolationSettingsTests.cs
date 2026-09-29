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

using System.Data;
using NUnit.Framework;
using Origam.DA;

namespace Origam.ServerTests;

[TestFixture]
public class DatabaseIsolationSettingsTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public void MissingSettingUsesReadCommitted(string value)
    {
        Assert.That(
            DatabaseIsolationSettings.Parse(value),
            Is.EqualTo(IsolationLevel.ReadCommitted)
        );
    }

    [TestCase("Snapshot", IsolationLevel.Snapshot)]
    [TestCase("read committed", IsolationLevel.ReadCommitted)]
    [TestCase("ReadUncommitted", IsolationLevel.ReadUncommitted)]
    [TestCase("RepeatableRead", IsolationLevel.RepeatableRead)]
    [TestCase("Serializable", IsolationLevel.Serializable)]
    public void ParsesSupportedIsolationLevel(string value, IsolationLevel expected)
    {
        Assert.That(DatabaseIsolationSettings.Parse(value), Is.EqualTo(expected));
    }

    [TestCase("Unspecified")]
    [TestCase("Chaos")]
    [TestCase("123")]
    [TestCase("4096")]
    [TestCase(" 4096 ")]
    [TestCase("2")]
    [TestCase("-1")]
    [TestCase("not-an-isolation-level")]
    public void RejectsUnsupportedIsolationLevel(string value)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            DatabaseIsolationSettings.Parse(value)
        );
        Assert.That(exception.Message, Does.Contain("Database:IsolationLevel"));
    }
}
