import axios, { AxiosHeaders } from "axios";

export default class BaseService {
  static BackendRoute: string | undefined = import.meta.env.VITE_BACKEND_URL;
  static GetUserStateEndpoint: string = "/Account/GetLoggedInUser";

  static async Get(
    route: string,
    additionalHeaders?: AxiosHeaders
  ): Promise<any> {
    return BaseService.BaseFetch("GET", route, undefined, additionalHeaders);
  }

  static async Post(
    route: string,
    body?: FormData,
    additionalHeaders?: AxiosHeaders
  ): Promise<any> {
    return BaseService.BaseFetch("POST", route, body, additionalHeaders);
  }

  static async Put(
    route: string,
    body?: FormData,
    additionalHeaders?: AxiosHeaders
  ): Promise<any> {
    return BaseService.BaseFetch("PUT", route, body, additionalHeaders);
  }

  static async Delete(
    route: string,
    body?: FormData,
    additionalHeaders?: AxiosHeaders
  ): Promise<any> {
    return BaseService.BaseFetch("DELETE", route, body, additionalHeaders);
  }

  //Todo: Sanitize query parts centrally
  static async BaseFetch(
    method: string,
    route: string,
    body?: FormData,
    additionalHeaders?: AxiosHeaders
  ): Promise<any> {
    if (!route.startsWith("/") && !route.startsWith("\\")) {
      route = "/" + route;
    }

    const response = await axios(`${BaseService.BackendRoute}${route}`, {
      method: method,
      withCredentials: import.meta.env.MODE !== "production",
      headers: additionalHeaders,
      data: body,
      validateStatus: () => true, //Disable throwing errors at 3xx, 4xx and 5xx status codes
      //Todo: Perhaps errors should be handled the axios way
    });

    const responseBody = response.data;

    if (import.meta.env.MODE !== "production") {
      console.log(responseBody);
    }

    if (response.status !== 200) {
      BaseService.ErrorHandling(route, response.status, responseBody);
    }

    return responseBody;
  }

  static ErrorHandling(route: string, status: number, body: any): void {
    //Todo: Proper user friendly error handling using floating popups or something
    if (body.error) {
      console.log(`Status Code: ${status}\nError: ${body.error}`);
    }

    if (
      (status === 401 || status === 403) &&
      !route.startsWith(BaseService.GetUserStateEndpoint)
    ) {
      window.location.replace("/login");
    }
  }
}
