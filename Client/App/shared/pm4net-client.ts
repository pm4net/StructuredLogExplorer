//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

/* tslint:disable */
/* eslint-disable */
// ReSharper disable InconsistentNaming

import { DateTime, Duration } from "luxon";

export interface IFileClient {

    getLogFileInfos(projectName: string | null | undefined): Promise<LogFileInfo[]>;

    importAll(projectName: string | null | undefined): Promise<{ [key: string]: LogFileInfo; }>;

    importLog(projectName: string | null | undefined, fileName: string | null | undefined): Promise<LogFileInfo | null>;
}

export class FileClient implements IFileClient {
    private http: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> };
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(baseUrl?: string, http?: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> }) {
        this.http = http ? http : window as any;
        this.baseUrl = baseUrl !== undefined && baseUrl !== null ? baseUrl : "";
    }

    getLogFileInfos(projectName: string | null | undefined): Promise<LogFileInfo[]> {
        let url_ = this.baseUrl + "/api/File/logFileInfos?";
        if (projectName !== undefined && projectName !== null)
            url_ += "projectName=" + encodeURIComponent("" + projectName) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processGetLogFileInfos(_response);
        });
    }

    protected processGetLogFileInfos(response: Response): Promise<LogFileInfo[]> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            if (Array.isArray(resultData200)) {
                result200 = [] as any;
                for (let item of resultData200)
                    result200!.push(LogFileInfo.fromJS(item));
            }
            else {
                result200 = <any>null;
            }
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<LogFileInfo[]>(null as any);
    }

    importAll(projectName: string | null | undefined): Promise<{ [key: string]: LogFileInfo; }> {
        let url_ = this.baseUrl + "/api/File/importAll?";
        if (projectName !== undefined && projectName !== null)
            url_ += "projectName=" + encodeURIComponent("" + projectName) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "POST",
            headers: {
                "Accept": "application/json"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processImportAll(_response);
        });
    }

    protected processImportAll(response: Response): Promise<{ [key: string]: LogFileInfo; }> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            if (resultData200) {
                result200 = {} as any;
                for (let key in resultData200) {
                    if (resultData200.hasOwnProperty(key))
                        (<any>result200)![key] = resultData200[key] ? LogFileInfo.fromJS(resultData200[key]) : new LogFileInfo();
                }
            }
            else {
                result200 = <any>null;
            }
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<{ [key: string]: LogFileInfo; }>(null as any);
    }

    importLog(projectName: string | null | undefined, fileName: string | null | undefined): Promise<LogFileInfo | null> {
        let url_ = this.baseUrl + "/api/File/importLog?";
        if (projectName !== undefined && projectName !== null)
            url_ += "projectName=" + encodeURIComponent("" + projectName) + "&";
        if (fileName !== undefined && fileName !== null)
            url_ += "fileName=" + encodeURIComponent("" + fileName) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "POST",
            headers: {
                "Accept": "application/json"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processImportLog(_response);
        });
    }

    protected processImportLog(response: Response): Promise<LogFileInfo | null> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? LogFileInfo.fromJS(resultData200) : <any>null;
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<LogFileInfo | null>(null as any);
    }
}

export interface IMapClient {

    getLogInfo(projectName: string | null | undefined): Promise<LogInfo>;

    discoverObjectCentricDirectlyFollowsGraph(projectName: string | null | undefined, minEvents: number | undefined, minOccurrences: number | undefined, minSuccessions: number | undefined, includedTypes: string[] | null | undefined): Promise<DirectedGraphOfNodeAndEdge>;

    discoverOcDfgAndGenerateDot(projectName: string | null | undefined, groupByNamespace: boolean | undefined, minEvents: number | undefined, minOccurrences: number | undefined, minSuccessions: number | undefined, includedTypes: string[] | null | undefined): Promise<string>;

    discoverOcDfgAndApplyStableGraphLayout(projectName: string | null | undefined, groupByNamespace: boolean | undefined, minEvents: number | undefined, minOccurrences: number | undefined, minSuccessions: number | undefined, includedTypes: string[] | null | undefined): Promise<string>;

    getNamespaceTree(projectName: string | null | undefined): Promise<ListTreeOfString>;
}

export class MapClient implements IMapClient {
    private http: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> };
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(baseUrl?: string, http?: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> }) {
        this.http = http ? http : window as any;
        this.baseUrl = baseUrl !== undefined && baseUrl !== null ? baseUrl : "";
    }

    getLogInfo(projectName: string | null | undefined): Promise<LogInfo> {
        let url_ = this.baseUrl + "/api/Map/getLogInfo?";
        if (projectName !== undefined && projectName !== null)
            url_ += "projectName=" + encodeURIComponent("" + projectName) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processGetLogInfo(_response);
        });
    }

    protected processGetLogInfo(response: Response): Promise<LogInfo> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = LogInfo.fromJS(resultData200);
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<LogInfo>(null as any);
    }

    discoverObjectCentricDirectlyFollowsGraph(projectName: string | null | undefined, minEvents: number | undefined, minOccurrences: number | undefined, minSuccessions: number | undefined, includedTypes: string[] | null | undefined): Promise<DirectedGraphOfNodeAndEdge> {
        let url_ = this.baseUrl + "/api/Map/discoverOcDfg?";
        if (projectName !== undefined && projectName !== null)
            url_ += "projectName=" + encodeURIComponent("" + projectName) + "&";
        if (minEvents === null)
            throw new Error("The parameter 'minEvents' cannot be null.");
        else if (minEvents !== undefined)
            url_ += "minEvents=" + encodeURIComponent("" + minEvents) + "&";
        if (minOccurrences === null)
            throw new Error("The parameter 'minOccurrences' cannot be null.");
        else if (minOccurrences !== undefined)
            url_ += "minOccurrences=" + encodeURIComponent("" + minOccurrences) + "&";
        if (minSuccessions === null)
            throw new Error("The parameter 'minSuccessions' cannot be null.");
        else if (minSuccessions !== undefined)
            url_ += "minSuccessions=" + encodeURIComponent("" + minSuccessions) + "&";
        if (includedTypes !== undefined && includedTypes !== null)
            includedTypes && includedTypes.forEach(item => { url_ += "includedTypes=" + encodeURIComponent("" + item) + "&"; });
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processDiscoverObjectCentricDirectlyFollowsGraph(_response);
        });
    }

    protected processDiscoverObjectCentricDirectlyFollowsGraph(response: Response): Promise<DirectedGraphOfNodeAndEdge> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = DirectedGraphOfNodeAndEdge.fromJS(resultData200);
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<DirectedGraphOfNodeAndEdge>(null as any);
    }

    discoverOcDfgAndGenerateDot(projectName: string | null | undefined, groupByNamespace: boolean | undefined, minEvents: number | undefined, minOccurrences: number | undefined, minSuccessions: number | undefined, includedTypes: string[] | null | undefined): Promise<string> {
        let url_ = this.baseUrl + "/api/Map/discoverOcDfgAndDot?";
        if (projectName !== undefined && projectName !== null)
            url_ += "projectName=" + encodeURIComponent("" + projectName) + "&";
        if (groupByNamespace === null)
            throw new Error("The parameter 'groupByNamespace' cannot be null.");
        else if (groupByNamespace !== undefined)
            url_ += "groupByNamespace=" + encodeURIComponent("" + groupByNamespace) + "&";
        if (minEvents === null)
            throw new Error("The parameter 'minEvents' cannot be null.");
        else if (minEvents !== undefined)
            url_ += "minEvents=" + encodeURIComponent("" + minEvents) + "&";
        if (minOccurrences === null)
            throw new Error("The parameter 'minOccurrences' cannot be null.");
        else if (minOccurrences !== undefined)
            url_ += "minOccurrences=" + encodeURIComponent("" + minOccurrences) + "&";
        if (minSuccessions === null)
            throw new Error("The parameter 'minSuccessions' cannot be null.");
        else if (minSuccessions !== undefined)
            url_ += "minSuccessions=" + encodeURIComponent("" + minSuccessions) + "&";
        if (includedTypes !== undefined && includedTypes !== null)
            includedTypes && includedTypes.forEach(item => { url_ += "includedTypes=" + encodeURIComponent("" + item) + "&"; });
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processDiscoverOcDfgAndGenerateDot(_response);
        });
    }

    protected processDiscoverOcDfgAndGenerateDot(response: Response): Promise<string> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 !== undefined ? resultData200 : <any>null;
    
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<string>(null as any);
    }

    discoverOcDfgAndApplyStableGraphLayout(projectName: string | null | undefined, groupByNamespace: boolean | undefined, minEvents: number | undefined, minOccurrences: number | undefined, minSuccessions: number | undefined, includedTypes: string[] | null | undefined): Promise<string> {
        let url_ = this.baseUrl + "/api/Map/discoverOcDfgAndApplyStableGraphLayout?";
        if (projectName !== undefined && projectName !== null)
            url_ += "projectName=" + encodeURIComponent("" + projectName) + "&";
        if (groupByNamespace === null)
            throw new Error("The parameter 'groupByNamespace' cannot be null.");
        else if (groupByNamespace !== undefined)
            url_ += "groupByNamespace=" + encodeURIComponent("" + groupByNamespace) + "&";
        if (minEvents === null)
            throw new Error("The parameter 'minEvents' cannot be null.");
        else if (minEvents !== undefined)
            url_ += "minEvents=" + encodeURIComponent("" + minEvents) + "&";
        if (minOccurrences === null)
            throw new Error("The parameter 'minOccurrences' cannot be null.");
        else if (minOccurrences !== undefined)
            url_ += "minOccurrences=" + encodeURIComponent("" + minOccurrences) + "&";
        if (minSuccessions === null)
            throw new Error("The parameter 'minSuccessions' cannot be null.");
        else if (minSuccessions !== undefined)
            url_ += "minSuccessions=" + encodeURIComponent("" + minSuccessions) + "&";
        if (includedTypes !== undefined && includedTypes !== null)
            includedTypes && includedTypes.forEach(item => { url_ += "includedTypes=" + encodeURIComponent("" + item) + "&"; });
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processDiscoverOcDfgAndApplyStableGraphLayout(_response);
        });
    }

    protected processDiscoverOcDfgAndApplyStableGraphLayout(response: Response): Promise<string> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 !== undefined ? resultData200 : <any>null;
    
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<string>(null as any);
    }

    getNamespaceTree(projectName: string | null | undefined): Promise<ListTreeOfString> {
        let url_ = this.baseUrl + "/api/Map/namespaceTree?";
        if (projectName !== undefined && projectName !== null)
            url_ += "projectName=" + encodeURIComponent("" + projectName) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processGetNamespaceTree(_response);
        });
    }

    protected processGetNamespaceTree(response: Response): Promise<ListTreeOfString> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = ListTreeOfString.fromJS(resultData200);
            return result200;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<ListTreeOfString>(null as any);
    }
}

export interface IProjectClient {

    create(name: string | null | undefined, logDir: string | null | undefined): Promise<void>;

    delete(name: string | null | undefined): Promise<void>;
}

export class ProjectClient implements IProjectClient {
    private http: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> };
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(baseUrl?: string, http?: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> }) {
        this.http = http ? http : window as any;
        this.baseUrl = baseUrl !== undefined && baseUrl !== null ? baseUrl : "";
    }

    create(name: string | null | undefined, logDir: string | null | undefined): Promise<void> {
        let url_ = this.baseUrl + "/api/Project/create?";
        if (name !== undefined && name !== null)
            url_ += "name=" + encodeURIComponent("" + name) + "&";
        if (logDir !== undefined && logDir !== null)
            url_ += "logDir=" + encodeURIComponent("" + logDir) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "GET",
            headers: {
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processCreate(_response);
        });
    }

    protected processCreate(response: Response): Promise<void> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            return;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<void>(null as any);
    }

    delete(name: string | null | undefined): Promise<void> {
        let url_ = this.baseUrl + "/api/Project/delete?";
        if (name !== undefined && name !== null)
            url_ += "name=" + encodeURIComponent("" + name) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "DELETE",
            headers: {
            }
        };

        return this.http.fetch(url_, options_).then((_response: Response) => {
            return this.processDelete(_response);
        });
    }

    protected processDelete(response: Response): Promise<void> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            return;
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<void>(null as any);
    }
}

export class LogFileInfo implements ILogFileInfo {
    id!: string;
    noOfImportedEvents!: number;
    noOfImportedObjects!: number;
    fileSize!: number;
    lastImported?: DateTime | undefined;
    lastChanged?: DateTime | undefined;

    constructor(data?: ILogFileInfo) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.id = _data["id"];
            this.noOfImportedEvents = _data["noOfImportedEvents"];
            this.noOfImportedObjects = _data["noOfImportedObjects"];
            this.fileSize = _data["fileSize"];
            this.lastImported = _data["lastImported"] ? DateTime.fromISO(_data["lastImported"].toString()) : <any>undefined;
            this.lastChanged = _data["lastChanged"] ? DateTime.fromISO(_data["lastChanged"].toString()) : <any>undefined;
        }
    }

    static fromJS(data: any): LogFileInfo {
        data = typeof data === 'object' ? data : {};
        let result = new LogFileInfo();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["noOfImportedEvents"] = this.noOfImportedEvents;
        data["noOfImportedObjects"] = this.noOfImportedObjects;
        data["fileSize"] = this.fileSize;
        data["lastImported"] = this.lastImported ? this.lastImported.toString() : <any>undefined;
        data["lastChanged"] = this.lastChanged ? this.lastChanged.toString() : <any>undefined;
        return data;
    }
}

export interface ILogFileInfo {
    id: string;
    noOfImportedEvents: number;
    noOfImportedObjects: number;
    fileSize: number;
    lastImported?: DateTime | undefined;
    lastChanged?: DateTime | undefined;
}

export class LogInfo implements ILogInfo {
    objectTypes!: string[];

    constructor(data?: ILogInfo) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.objectTypes = [];
        }
    }

    init(_data?: any) {
        if (_data) {
            if (Array.isArray(_data["objectTypes"])) {
                this.objectTypes = [] as any;
                for (let item of _data["objectTypes"])
                    this.objectTypes!.push(item);
            }
        }
    }

    static fromJS(data: any): LogInfo {
        data = typeof data === 'object' ? data : {};
        let result = new LogInfo();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (Array.isArray(this.objectTypes)) {
            data["objectTypes"] = [];
            for (let item of this.objectTypes)
                data["objectTypes"].push(item);
        }
        return data;
    }
}

export interface ILogInfo {
    objectTypes: string[];
}

export class DirectedGraphOfNodeAndEdge implements IDirectedGraphOfNodeAndEdge {
    nodes!: Node[];
    edges!: ValueTupleOfNodeAndNodeAndEdge[];

    constructor(data?: IDirectedGraphOfNodeAndEdge) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.nodes = [];
            this.edges = [];
        }
    }

    init(_data?: any) {
        if (_data) {
            if (Array.isArray(_data["nodes"])) {
                this.nodes = [] as any;
                for (let item of _data["nodes"])
                    this.nodes!.push(Node.fromJS(item));
            }
            if (Array.isArray(_data["edges"])) {
                this.edges = [] as any;
                for (let item of _data["edges"])
                    this.edges!.push(ValueTupleOfNodeAndNodeAndEdge.fromJS(item));
            }
        }
    }

    static fromJS(data: any): DirectedGraphOfNodeAndEdge {
        data = typeof data === 'object' ? data : {};
        let result = new DirectedGraphOfNodeAndEdge();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (Array.isArray(this.nodes)) {
            data["nodes"] = [];
            for (let item of this.nodes)
                data["nodes"].push(item.toJSON());
        }
        if (Array.isArray(this.edges)) {
            data["edges"] = [];
            for (let item of this.edges)
                data["edges"].push(item.toJSON());
        }
        return data;
    }
}

export interface IDirectedGraphOfNodeAndEdge {
    nodes: Node[];
    edges: ValueTupleOfNodeAndNodeAndEdge[];
}

export abstract class Node implements INode {

    protected _discriminator: string;

    constructor(data?: INode) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        this._discriminator = "Node";
    }

    init(_data?: any) {
    }

    static fromJS(data: any): Node {
        data = typeof data === 'object' ? data : {};
        if (data["discriminator"] === "StartNode") {
            let result = new StartNode();
            result.init(data);
            return result;
        }
        if (data["discriminator"] === "EndNode") {
            let result = new EndNode();
            result.init(data);
            return result;
        }
        if (data["discriminator"] === "EventNode") {
            let result = new EventNode();
            result.init(data);
            return result;
        }
        throw new Error("The abstract class 'Node' cannot be instantiated.");
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["discriminator"] = this._discriminator;
        return data;
    }
}

export interface INode {
}

export class StartNode extends Node implements IStartNode {
    type!: string;

    constructor(data?: IStartNode) {
        super(data);
        this._discriminator = "StartNode";
    }

    override init(_data?: any) {
        super.init(_data);
        if (_data) {
            this.type = _data["type"];
        }
    }

    static override fromJS(data: any): StartNode {
        data = typeof data === 'object' ? data : {};
        let result = new StartNode();
        result.init(data);
        return result;
    }

    override toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["type"] = this.type;
        super.toJSON(data);
        return data;
    }
}

export interface IStartNode extends INode {
    type: string;
}

export class EndNode extends Node implements IEndNode {
    type!: string;

    constructor(data?: IEndNode) {
        super(data);
        this._discriminator = "EndNode";
    }

    override init(_data?: any) {
        super.init(_data);
        if (_data) {
            this.type = _data["type"];
        }
    }

    static override fromJS(data: any): EndNode {
        data = typeof data === 'object' ? data : {};
        let result = new EndNode();
        result.init(data);
        return result;
    }

    override toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["type"] = this.type;
        super.toJSON(data);
        return data;
    }
}

export interface IEndNode extends INode {
    type: string;
}

export class EventNode extends Node implements IEventNode {
    name!: string;
    level!: LogLevel;
    namespace!: string;
    statistics!: NodeStatistics;

    constructor(data?: IEventNode) {
        super(data);
        if (!data) {
            this.statistics = new NodeStatistics();
        }
        this._discriminator = "EventNode";
    }

    override init(_data?: any) {
        super.init(_data);
        if (_data) {
            this.name = _data["name"];
            this.level = _data["level"];
            this.namespace = _data["namespace"];
            this.statistics = _data["statistics"] ? NodeStatistics.fromJS(_data["statistics"]) : new NodeStatistics();
        }
    }

    static override fromJS(data: any): EventNode {
        data = typeof data === 'object' ? data : {};
        let result = new EventNode();
        result.init(data);
        return result;
    }

    override toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["level"] = this.level;
        data["namespace"] = this.namespace;
        data["statistics"] = this.statistics ? this.statistics.toJSON() : <any>undefined;
        super.toJSON(data);
        return data;
    }
}

export interface IEventNode extends INode {
    name: string;
    level: LogLevel;
    namespace: string;
    statistics: NodeStatistics;
}

export enum LogLevel {
    Verbose = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Fatal = 5,
    Unknown = 6,
}

export class NodeStatistics implements INodeStatistics {
    frequency!: number;

    constructor(data?: INodeStatistics) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.frequency = _data["frequency"];
        }
    }

    static fromJS(data: any): NodeStatistics {
        data = typeof data === 'object' ? data : {};
        let result = new NodeStatistics();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["frequency"] = this.frequency;
        return data;
    }
}

export interface INodeStatistics {
    frequency: number;
}

export class ValueTupleOfNodeAndNodeAndEdge implements IValueTupleOfNodeAndNodeAndEdge {
    item1!: Node;
    item2!: Node;
    item3!: Edge;

    constructor(data?: IValueTupleOfNodeAndNodeAndEdge) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.item3 = new Edge();
        }
    }

    init(_data?: any) {
        if (_data) {
            this.item1 = _data["item1"] ? Node.fromJS(_data["item1"]) : <any>undefined;
            this.item2 = _data["item2"] ? Node.fromJS(_data["item2"]) : <any>undefined;
            this.item3 = _data["item3"] ? Edge.fromJS(_data["item3"]) : new Edge();
        }
    }

    static fromJS(data: any): ValueTupleOfNodeAndNodeAndEdge {
        data = typeof data === 'object' ? data : {};
        let result = new ValueTupleOfNodeAndNodeAndEdge();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["item1"] = this.item1 ? this.item1.toJSON() : <any>undefined;
        data["item2"] = this.item2 ? this.item2.toJSON() : <any>undefined;
        data["item3"] = this.item3 ? this.item3.toJSON() : <any>undefined;
        return data;
    }
}

export interface IValueTupleOfNodeAndNodeAndEdge {
    item1: Node;
    item2: Node;
    item3: Edge;
}

export class Edge implements IEdge {
    type!: string;
    statistics!: EdgeStatistics;

    constructor(data?: IEdge) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.statistics = new EdgeStatistics();
        }
    }

    init(_data?: any) {
        if (_data) {
            this.type = _data["type"];
            this.statistics = _data["statistics"] ? EdgeStatistics.fromJS(_data["statistics"]) : new EdgeStatistics();
        }
    }

    static fromJS(data: any): Edge {
        data = typeof data === 'object' ? data : {};
        let result = new Edge();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["type"] = this.type;
        data["statistics"] = this.statistics ? this.statistics.toJSON() : <any>undefined;
        return data;
    }
}

export interface IEdge {
    type: string;
    statistics: EdgeStatistics;
}

export class EdgeStatistics implements IEdgeStatistics {
    frequency!: number;

    constructor(data?: IEdgeStatistics) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.frequency = _data["frequency"];
        }
    }

    static fromJS(data: any): EdgeStatistics {
        data = typeof data === 'object' ? data : {};
        let result = new EdgeStatistics();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["frequency"] = this.frequency;
        return data;
    }
}

export interface IEdgeStatistics {
    frequency: number;
}

export class ListTreeOfString implements IListTreeOfString {

    constructor(data?: IListTreeOfString) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
    }

    static fromJS(data: any): ListTreeOfString {
        data = typeof data === 'object' ? data : {};
        let result = new ListTreeOfString();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        return data;
    }
}

export interface IListTreeOfString {
}

export class SwaggerException extends Error {
    override message: string;
    status: number;
    response: string;
    headers: { [key: string]: any; };
    result: any;

    constructor(message: string, status: number, response: string, headers: { [key: string]: any; }, result: any) {
        super();

        this.message = message;
        this.status = status;
        this.response = response;
        this.headers = headers;
        this.result = result;
    }

    protected isSwaggerException = true;

    static isSwaggerException(obj: any): obj is SwaggerException {
        return obj.isSwaggerException === true;
    }
}

function throwException(message: string, status: number, response: string, headers: { [key: string]: any; }, result?: any): any {
    if (result !== null && result !== undefined)
        throw result;
    else
        throw new SwaggerException(message, status, response, headers, null);
}