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

using NUnit.Framework;

namespace Origam.Common_net2Tests;

[TestFixture]
public class HttpToolsCookieTests
{
    [TestCase(
        arg1: "laravel_session=YmY3NDk3NTgwZmZhU5NDU5NjExZTQifQ%3D%3D; expires=Thu, 11-Jan-2018 17:50:21 GMT; Max-Age=7200; path=/; HttpOnly",
        arg2: 1
    )]
    [TestCase(
        arg1: "laravel_session=YmY3NDk3NTgwZmZhU5NDU5NjExZTQifQ%3D%3D; expires=Thu, 11-Jan-2018 17:50:21 GMT; Max-Age=7200; path=/; HttpOnly, "
            + "laravel_session=YmY3NDk3NTgwZmZhU5NDU5NjExZTQifQ%3D%3D; expires=Thu, 11-Jan-2018 17:50:21 GMT; Max-Age=7200; path=/; HttpOnly",
        arg2: 2
    )]
    public void ShouldNotSplitASingleCookieWithAnExpiresAttribute(
        string setHeaderStr,
        int numOfCookiesInside
    )
    {
        var sut = HttpTools.Instance;
        var cookies = sut.SplitCookiesHeaderToSingleCookies(setCookieHeader: setHeaderStr);
        Assert.That(actual: cookies.Count, expression: Is.EqualTo(expected: numOfCookiesInside));
    }

    [Test]
    public void ShouldParseStringToACookie()
    {
        var singleCookie = new System.Collections.Generic.List<string>
        {
            "laravel_session=YmY3NDk3NTgwZmZhU5NDU5NjExZTQifQ%3D%3D; expires=Thu, 11-Jan-2018 17:50:21 GMT; Max-Age=7200; path=/; HttpOnly",
        };
        var sut = HttpTools.Instance;
        var cookies = sut.CookiesFromStrings(host: "test", singleCookies: singleCookie);
        var cookie = cookies[index: 0];

        // We are only interested in name and value.
        // Rest of the cookie is ignored.
        Assert.That(actual: cookie.Name, expression: Is.EqualTo(expected: "laravel_session"));
        Assert.That(
            actual: cookie.Value,
            expression: Is.EqualTo(expected: "YmY3NDk3NTgwZmZhU5NDU5NjExZTQifQ%3D%3D")
        );
    }
}
