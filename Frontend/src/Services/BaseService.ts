export default class BaseService {
  static async Get(route: string, additionalHeaders?: Headers) {
    return BaseService.BaseFetch("GET", route, undefined, additionalHeaders);
  }

  static async Post(
    route: string,
    body?: FormData,
    additionalHeaders?: Headers
  ) {
    return BaseService.BaseFetch("POST", route, body, additionalHeaders);
  }

  static async Put(
    route: string,
    body?: FormData,
    additionalHeaders?: Headers
  ) {
    return BaseService.BaseFetch("PUT", route, body, additionalHeaders);
  }

  static async Delete(
    route: string,
    body?: FormData,
    additionalHeaders?: Headers
  ) {
    return BaseService.BaseFetch("DELETE", route, body, additionalHeaders);
  }

  static async BaseFetch(
    method: string,
    route: string,
    body?: FormData,
    additionalHeaders?: Headers
  ) {
    if (!route.startsWith("/") && !route.startsWith("\\")) {
      route = "/" + route;
    }

    const response = await fetch(
      `${process.env.REACT_APP_BACKEND_URL}${route}`,
      {
        method: method,
        credentials: "include",
        headers: additionalHeaders,
        body: body,
      }
    );

    const responseBody = await response.json();

    if (process.env.NODE_ENV === "development") {
      console.log(response.status);
      console.log(responseBody);
    }

    if (response.status !== 200) {
      BaseService.ErrorHandling(route, response.status, responseBody);
    }

    return responseBody;
  }

  static ErrorHandling(route: string, status: number, body: any) {
    if (body.error) {
      console.log(`Status Code: ${status}\nError: ${body.error}`);
    }

    if (status === 401 && !route.startsWith("/Account/GetLoggedInUser")) {
      window.location.reload();
    }
  }
}
