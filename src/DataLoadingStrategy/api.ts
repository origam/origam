import { IAPI } from "./types";
import axios from "axios";

let globalAuthToken = sessionStorage.getItem("origamAccessToken") || "";

export function getToken() {
  return globalAuthToken;
}

function login() {
  const USER = "PTLE540\\pavel";
  const PASS = "PTLE540\\pavel";
  const params = new URLSearchParams();
  params.append("username", USER);
  params.append("password", PASS);
  if (globalAuthToken) {
    return Promise.resolve(globalAuthToken);
  }
  return axios
    .post("/api/Token/Create", params, {
      headers: {
        "Content-Type": "application/x-www-form-urlencoded"
      }
    })
    .then(response => {
      sessionStorage.setItem("origamAccessToken", response.data);
      return (globalAuthToken = response.data);
    })
}

export class APIFake implements IAPI {
  public loadDataTable({
    tableId,
    columns,
    limit,
    filter,
    orderBy,
    token
  }: {
    tableId: string;
    token: string;
    columns?: string[] | undefined;
    limit?: number | undefined;
    filter?: Array<[string, string, string]> | undefined;
    orderBy?: Array<[string, string]> | undefined;
  }): Promise<any> {
    return axios.get(`http://127.0.0.1:8080/api/${tableId}`, {
      params: {
        limit,
        cols: JSON.stringify(columns),
        filter: JSON.stringify(filter),
        odb: orderBy && orderBy.length > 0 ? JSON.stringify(orderBy) : undefined
      }
    });
  }

  public loadMenu({ token }: { token: string }): Promise<any> {
    return axios.get("/menu01.xml");
  }

  public loadScreen({
    id,
    token
  }: {
    id: string;
    token: string;
  }): Promise<any> {
    return axios.get(
      {
        "b2a2194c-c2a5-4236-837a-08c1b720fc96": "/screen01.xml",
        "f96ad0ca-7be8-4f11-a397-358d02abd0b2": "/screen02.xml",
        "8713c618-b4fb-4749-ab28-0811b85481b0": "/screen03.xml",
        "3272db8a-172f-48ee-9e50-ef65219089c2": "/screen04.xml"
      }[id]
    );
  }
}

export class APIOrigam implements IAPI {
  public loadDataTable({
    tableId,
    columns,
    limit,
    filter,
    orderBy,
    token,
    menuId
  }: {
    tableId: string;
    token: string;
    columns?: string[] | undefined;
    limit?: number | undefined;
    filter?: Array<[string, string, string]> | undefined;
    orderBy?: Array<[string, string]> | undefined;
    menuId: string;
  }): Promise<any> {
    return login().then(fToken =>
      axios.post(
        `/api/Data/EntitiesGet`,
        {
          dataStructureEntityId: tableId,
          filter: filter ? JSON.stringify(filter) : "",
          ordering: orderBy || "",
          rowLimit: `${limit}`,
          columnNames: columns,
          menuId
        },
        { headers: { Authorization: `Bearer ${fToken}` } }
      )
    );
  }

  public loadMenu({ token }: { token: string }) {
    return login().then(fToken =>
      axios.get("/api/MetaData/GetMenu", {
        headers: { Authorization: `Bearer ${fToken}` }
      })
    );
  }

  public loadScreen({
    id,
    token
  }: {
    id: string;
    token: string;
  }): Promise<any> {
    return login().then(fToken =>
      axios.get(`/api/MetaData/GetScreeSection`, {
        headers: { Authorization: `Bearer ${fToken}` },
        params: { id }
      })
    );
  }
}

const api = new APIOrigam();

export { api };
