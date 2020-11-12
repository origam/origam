import { openScreenByReference } from "../Workbench/openScreenByReference";

export function executeWeblink(ctx: any) {
  return function* executeWeblink(urlPath: string, urlQuery?: { [key: string]: any }): Generator {
    switch (urlPath) {
      case "openOneRecordWindow":
        {
          const referenceId = urlQuery?.referenceId;
          const lookupId = urlQuery?.lookupId;
          if (referenceId && lookupId) {
            yield* openScreenByReference(ctx)(lookupId, referenceId);
          }
        }
        break;
    }
  };
}
