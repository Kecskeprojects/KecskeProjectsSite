export default class BaseService {
  static async Get(route, additionalHeaders = {}) {
    return BaseService.BaseFetch("GET", route, null, additionalHeaders);
  }

  static async Post(route, body = {}, additionalHeaders = {}) {
    return BaseService.BaseFetch("POST", route, body, additionalHeaders);
  }

  static async Put(route, body = {}, additionalHeaders = {}) {
    return BaseService.BaseFetch("PUT", route, body, additionalHeaders);
  }

  static async Delete(route, body = {}, additionalHeaders = {}) {
    return BaseService.BaseFetch("DELETE", route, body, additionalHeaders);
  }

  static async BaseFetch(method, route, body, additionalHeaders) {
    if (!route.startsWith("/") && !route.startsWith("\\")) {
      route = "/" + route;
    }

    const response = await fetch(
      `${process.env.REACT_APP_BACKEND_URL}${route}`,
      {
        method: method,
        credentials: "include",
        headers: new Headers({
          ...additionalHeaders,
        }),
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

  static ErrorHandling(route, status, body) {
    if (body.error) {
      console.log(`Status Code: ${status}\nError: ${body.error}`);
    }

    if (status === 401 && !route.startsWith("/Account/GetLoggedInUser")) {
      window.location.reload();
    }
  }
}
