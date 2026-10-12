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

import fs from 'node:fs';
import path from 'node:path';
import { expect, test, type APIRequestContext, type Page, type Response } from '@playwright/test';
import { modelFilePath, reloadBackend, resetBackend } from '@support/resetBackend';

const TRANSFORMATION_ID = '7d3f2a10-5b8e-4c61-9f0a-2e4b6c8d1a35';
const TRANSFORMATION_NAME = 'XsltInputParametersTest';
const TRANSFORMATION_DIRECTORY = modelFilePath('Root/Transformation');

function stylesheet(content: string): string {
  return `<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions" exclude-result-prefixes="AS">
${content}
</xsl:stylesheet>
`;
}

const NO_PARAMETERS_XSL = stylesheet(`
	<xsl:template match="ROOT">
		<ROOT/>
	</xsl:template>`);

const UNTYPED_PARAMETER_XSL = stylesheet(`
	<xsl:param name="greeting"/>

	<xsl:template match="ROOT">
		<ROOT>
			<Greeting><xsl:value-of select="$greeting"/></Greeting>
		</ROOT>
	</xsl:template>`);

const TYPED_PARAMETERS_XSL = stylesheet(`
	<xsl:param name="isActive" AS:DataType="Boolean"/>
	<xsl:param name="count" AS:DataType="Integer"/>
	<xsl:param name="items" AS:DataType="Xml"/>
	<xsl:param name="note"/>

	<xsl:template match="ROOT">
		<ROOT>
			<Active><xsl:value-of select="$isActive"/></Active>
			<Next><xsl:value-of select="$count + 1"/></Next>
			<ItemCount><xsl:value-of select="count($items/items/item)"/></ItemCount>
			<Note><xsl:value-of select="$note"/></Note>
		</ROOT>
	</xsl:template>`);

const TYPED_DEFAULTS_XSL = stylesheet(`
	<xsl:param name="isActive" AS:DataType="Boolean" select="false()"/>
	<xsl:param name="count" AS:DataType="Integer" select="1"/>
	<xsl:param name="items" AS:DataType="Xml" select="/.."/>

	<xsl:template match="ROOT">
		<ROOT>
			<Active><xsl:value-of select="$isActive"/></Active>
			<Count><xsl:value-of select="$count"/></Count>
			<ItemCount><xsl:value-of select="count($items/items/item)"/></ItemCount>
		</ROOT>
	</xsl:template>`);

const UNKNOWN_TYPE_XSL = stylesheet(`
	<xsl:param name="day" AS:DataType="date"/>

	<xsl:template match="ROOT">
		<ROOT/>
	</xsl:template>`);

const INVALID_XSL = stylesheet(`
	<xsl:param name="greeting"/>
	<xsl:template match="ROOT">`);

const TYPED_PARAMETERS = [
  { name: 'isActive', type: 'Boolean', value: 'true', result: '<Active>true</Active>' },
  { name: 'count', type: 'Integer', value: '41', result: '<Next>42</Next>' },
  {
    name: 'items',
    type: 'Xml',
    value: '<items>\n<item/>\n<item/>\n</items>',
    result: '<ItemCount>2</ItemCount>',
  },
  { name: 'note', type: 'String', value: 'typed', result: '<Note>typed</Note>' },
];

async function plantTransformation(request: APIRequestContext, xsl: string): Promise<void> {
  const textFile = `${TRANSFORMATION_NAME}.origam___text___${TRANSFORMATION_ID}.xslt`;
  const definition = `<?xml version="1.0" encoding="utf-8"?>
<x:file
  xmlns:x="http://schemas.origam.com/model-persistence/1.0.0"
  xmlns:asi="http://schemas.origam.com/Origam.Schema.AbstractSchemaItem/6.0.0"
  xmlns:xt="http://schemas.origam.com/Origam.Schema.EntityModel.XslTransformation/6.0.1">
  <xt:Transformation
    asi:abstract="false"
    x:id="${TRANSFORMATION_ID}"
    asi:name="${TRANSFORMATION_NAME}"
    xt:text="-***-ExternalFile:${textFile}" />
</x:file>
`;
  fs.writeFileSync(path.join(TRANSFORMATION_DIRECTORY, textFile), xsl, 'utf8');
  fs.writeFileSync(
    path.join(TRANSFORMATION_DIRECTORY, `${TRANSFORMATION_NAME}.origam`),
    definition,
    'utf8',
  );
  await reloadBackend(request, 'Root');
}

function visibleCodeEditor(page: Page) {
  return page.getByTestId('code-editor').filter({ visible: true }).locator('.view-lines');
}

async function openTransformation(page: Page): Promise<void> {
  await page.goto('/');
  await page.getByTestId('tree-toggle-Business Logic').click();
  await page.getByTestId('tree-toggle-Transformations').click();
  await page.getByTestId(`tree-node-${TRANSFORMATION_NAME}`).dblclick();
  await expect(visibleCodeEditor(page)).toContainText('<xsl:stylesheet', { timeout: 30_000 });
}

async function openInputParameters(page: Page): Promise<Response> {
  const parametersLoaded = page.waitForResponse(response =>
    response.url().includes('/Xslt/Parameters'),
  );
  await page.getByText('Input Parameters', { exact: true }).click();
  return parametersLoaded;
}

function parameterValueEditor(page: Page) {
  return page.getByTestId('xslt-parameter-value').locator('.view-lines');
}

async function selectParameter(page: Page, name: string): Promise<void> {
  await page.getByTestId(`xslt-parameter-${name}`).click();
}

async function setParameterValue(page: Page, value: string): Promise<void> {
  await parameterValueEditor(page).click();
  await page.keyboard.press('Control+A');
  await page.keyboard.press('Delete');
  if (value) {
    // insertText bypasses Monaco's tag auto-closing
    await page.keyboard.insertText(value);
  }
}

async function transform(page: Page): Promise<void> {
  const transformed = page.waitForResponse(response => response.url().includes('/Xslt/Transform'));
  await page.getByText('Transform', { exact: true }).filter({ visible: true }).click();
  await transformed;
}

async function breakXsl(page: Page): Promise<void> {
  await visibleCodeEditor(page).click();
  await page.keyboard.press('Control+End');
  const propertyUpdated = page.waitForResponse(
    response => response.url().includes('/PropertyEditor/Update') && response.ok(),
  );
  await page.keyboard.type('<');
  await propertyUpdated;
}

test.describe('Input parameters of the XSLT editor (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('lists an untyped parameter as String', async ({ page, request }) => {
    await plantTransformation(request, UNTYPED_PARAMETER_XSL);
    await openTransformation(page);

    const response = await openInputParameters(page);

    expect(response.status()).toBe(200);
    await expect(page.getByTestId('xslt-parameter-type')).toHaveValue('String');
    await expect(parameterValueEditor(page)).toHaveText('');
  });

  test('passes an entered value to the transformation', async ({ page, request }) => {
    await plantTransformation(request, UNTYPED_PARAMETER_XSL);
    await openTransformation(page);
    await openInputParameters(page);

    await setParameterValue(page, 'Hello from E2E');
    await transform(page);

    await expect(visibleCodeEditor(page)).toContainText('<Greeting>Hello from E2E</Greeting>');
  });

  test('lists parameters that declare AS:DataType', async ({ page, request }) => {
    await plantTransformation(request, TYPED_PARAMETERS_XSL);
    await openTransformation(page);

    const response = await openInputParameters(page);

    expect(response.status()).toBe(200);
    for (const { name } of TYPED_PARAMETERS) {
      await expect(page.getByTestId(`xslt-parameter-${name}`)).toBeVisible();
    }
  });

  test('preselects the type declared by AS:DataType', async ({ page, request }) => {
    await plantTransformation(request, TYPED_PARAMETERS_XSL);
    await openTransformation(page);

    await openInputParameters(page);

    for (const { name, type } of TYPED_PARAMETERS) {
      await selectParameter(page, name);
      await expect(page.getByTestId('xslt-parameter-type')).toHaveValue(type);
    }
  });

  test('passes typed values to the transformation', async ({ page, request }) => {
    await plantTransformation(request, TYPED_PARAMETERS_XSL);
    await openTransformation(page);
    await openInputParameters(page);

    for (const { name, type, value } of TYPED_PARAMETERS) {
      await selectParameter(page, name);
      await page.getByTestId('xslt-parameter-type').selectOption(type);
      await setParameterValue(page, value);
    }
    await transform(page);

    for (const { result } of TYPED_PARAMETERS) {
      await expect(visibleCodeEditor(page)).toContainText(result);
    }
  });

  test('reports an unknown AS:DataType', async ({ page, request }) => {
    await plantTransformation(request, UNKNOWN_TYPE_XSL);
    await openTransformation(page);

    const response = await openInputParameters(page);

    expect(response.status()).toBe(420);
    await expect(page.getByText('Parameter type date is not OrigamDataType.')).toBeVisible();
  });

  for (const { caption, value } of [
    { caption: 'empty', value: '' },
    { caption: 'blank', value: ' ' },
  ]) {
    test(`uses XSL defaults for typed parameters left ${caption}`, async ({ page, request }) => {
      await plantTransformation(request, TYPED_DEFAULTS_XSL);
      await openTransformation(page);
      await openInputParameters(page);
      for (const name of ['isActive', 'count', 'items']) {
        await selectParameter(page, name);
        await setParameterValue(page, value);
      }

      await transform(page);

      const result = visibleCodeEditor(page);
      await expect(result).toContainText('<Active>false</Active>');
      await expect(result).toContainText('<Count>1</Count>');
      await expect(result).toContainText('<ItemCount>0</ItemCount>');
    });
  }

  test('does not report missing parameters for an invalid XSL', async ({ page, request }) => {
    await plantTransformation(request, INVALID_XSL);
    await openTransformation(page);

    const response = await openInputParameters(page);

    expect(response.status()).toBe(200);
    await expect(page.getByTestId('output')).not.toBeEmpty();
    await expect(page.getByTestId('xslt-parameters-empty')).toHaveCount(0);
  });

  test('clears the parameters when the XSL becomes invalid', async ({ page, request }) => {
    await plantTransformation(request, UNTYPED_PARAMETER_XSL);
    await openTransformation(page);
    await openInputParameters(page);
    await expect(page.getByTestId('xslt-parameter-greeting')).toBeVisible();

    await page.getByText('XSL', { exact: true }).click();
    await breakXsl(page);
    await openInputParameters(page);

    await expect(page.getByTestId('output')).not.toBeEmpty();
    await expect(page.getByTestId('xslt-parameter-greeting')).toHaveCount(0);
    await expect(page.getByTestId('xslt-parameters-empty')).toHaveCount(0);
  });

  test('explains how to declare a parameter when there is none', async ({ page, request }) => {
    await plantTransformation(request, NO_PARAMETERS_XSL);
    await openTransformation(page);

    const response = await openInputParameters(page);

    expect(response.status()).toBe(200);
    await expect(page.getByTestId('xslt-parameters-empty')).toContainText('xsl:param');
  });
});
