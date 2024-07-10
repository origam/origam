#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Origam.ServerCore.Controller;

namespace Origam.ServerCoreTests;
[TestFixture]
class UIControllerTests: ControllerTests
{
    private UIController sut;
    private Guid sessionId;
    private Guid rowId;
    public UIControllerTests()
    {
        sut = new UIController(new NullLogger<UIController>());
        sut.ControllerContext = context;
    }
    [Test, Order(201)]
    public void ShouldReturnMenuXml()
    {
        var actionResult = sut.GetMenu();
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        OkObjectResult okObjectResult = (OkObjectResult)actionResult;
        Assert.IsInstanceOf<string>(okObjectResult.Value);
        string menuXml = (string)okObjectResult.Value;
        Assert.That(menuXml, Is.Not.Empty);
    }
    [Test, Order(202)]
    public void ShouldReturnScreenXml()
    {
        var actionResult = sut.GetUI(new Guid("f38fdadb-4bba-4bb7-8184-c9109d5d40cd"));
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        OkObjectResult okObjectResult = (OkObjectResult)actionResult;
        Assert.IsInstanceOf<string>(okObjectResult.Value);
        string menuXml = (string)okObjectResult.Value;
        Assert.That(menuXml, Is.Not.Empty);
    }
}
