/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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

import {getCommonTabIndex, TabIndex} from "../model/entities/TabIndexOwner";

test.only("Get common tabIndex", () =>{
    const owners = [
        {tabIndex: TabIndex.create("0.1.0.10")},
        {tabIndex: TabIndex.create("0.1.0.11")},
        {tabIndex: TabIndex.create("0.1.0.12")},
    ];
    const commonTabIndex = getCommonTabIndex(owners);
    expect(commonTabIndex.toString()).toBe("0.1.0");
});

test.only("Should ignore undefined when looking for common index", () =>{
    const owners = [
        {tabIndex: TabIndex.create(undefined)},
        {tabIndex: TabIndex.create("0.1.0.11")},
        {tabIndex: TabIndex.create("0.1.0.12")},
    ];
    const commonTabIndex = getCommonTabIndex(owners);
    expect(commonTabIndex.toString()).toBe("0.1.0");
});