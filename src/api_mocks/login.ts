import MockAdapter from "axios-mock-adapter";

export function mockLogin(mock: MockAdapter) {
  return mock
    .onPost("/internalApi/User/Login")
    .reply(
      200,
      `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJuYmYiOiIxNTc4NDA2NjUzIiwiZXhwIjoiMTU3ODQ5MzA1MyJ9.XjWyIdieCssnm0HWBGr3qk-lLtE1NAZKx-lQaFBOOqc`
    );
}
