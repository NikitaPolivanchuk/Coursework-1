using System.Reflection;
using Webserver.Utility;

namespace Webserver
{
    internal class Router
    {
        private class Method
        {
            public Type CallerType { get; }
            public MethodInfo Function { get; }

            public Method(Type caller, MethodInfo function)
            {
                CallerType = caller;
                Function = function;
            }
        }
        private class Node
        {
            public string Name { get; }
            public Method? Method { get; set; }
            private Dictionary<string, Node>? SubNodes { get; set; }
            private Dictionary<Type, Node>? ArgSubNodes { get; set; }
            
            public Node(string name)
            {
                Name = name;
            } 

            public void AddSubNode(string name, Node node)
            {
                if (SubNodes == null)
                {
                    SubNodes = new Dictionary<string, Node>();
                }
                SubNodes[name] = node;
            }

            public void AddArgSubNode(Type type, Node node)
            {
                if (ArgSubNodes == null)
                {
                    ArgSubNodes = new Dictionary<Type, Node>();
                }
                ArgSubNodes[type] = node;
            }

            public bool TryGetSubNode(string name, out Node? node)
            {
                if (SubNodes != null && SubNodes.TryGetValue(name, out node))
                {
                    return true;
                }
                node = this;
                return false;
            }

            public bool TryGetArgSubNode(Type type, out Node? node)
            {
                if (ArgSubNodes != null && ArgSubNodes.TryGetValue(type, out node))
                {
                    return true;
                }
                node = this;
                return false;
            }
        }


        private Node? root;

        private Dictionary<Type, object>? callers;


        public void AddCaller(Type callerType, object caller)
        {
            if (callers == null)
            {
                callers = new Dictionary<Type, object>();
            }
            callers[callerType] = caller;
        }


        public void AddRoute(HttpMethod httpMethod, string route, Type caller, MethodInfo function)
        {
            if (root == null)
            {
                root = new Node("/");
            }

            Node current = root;

            route = httpMethod + "/" + route;

            foreach (string element in route.Split('/'))
            {
                if (string.IsNullOrEmpty(element)) continue;

                Node? next = null;

                if (element.StartsWith('{') && element.EndsWith('}'))
                {
                    string param = element.Substring(1, element.Length - 2);
                    int index = param.IndexOf(':');

                    string name = string.Empty;
                    string typeStr;

                    if (index != -1)
                    {
                        name = param.Substring(0, index);
                        typeStr = param.Substring(index + 1);
                    }
                    else
                    {
                        name = param;
                        typeStr = "string";
                    }

                    ArgType argType = ArgType.GetArgType(typeStr);

                    next = current.TryGetArgSubNode(argType.Type, out Node? argNode)
                        ? argNode 
                        : new Node(name);

                    current.AddArgSubNode(argType.Type, next!);
                }
                else
                {
                    next = current!.TryGetSubNode(element, out Node? node)
                    ? node
                    : new Node(element);

                    current.AddSubNode(element, next!);
                }

                current = next!;
            }
            current.Method = new Method(caller, function);
        }

        public async Task<ResponseAction> TryRoute(Session session, HttpMethod httpMethod, string? route, Dictionary<string, string>? inputParams)
        {
            if (root == null)
            {
                return await Task.FromResult(new ResponseAction(false, null, null));
            }

            route = httpMethod + route;

            Dictionary<string, string> queryParams = null;
            int paramIdx = route.IndexOf('?');
            if (paramIdx != -1)
            {
                queryParams = route.Substring(paramIdx + 1).asQuery();
                route = route.Substring(0, paramIdx);
            }

            if (inputParams != null)
            {
                if (queryParams == null)
                {
                    queryParams = inputParams;
                }
                else
                {
                    foreach (var pair in inputParams)
                    {
                        queryParams[pair.Key] = pair.Value;
                    }
                }
            }

            if (queryParams == null)
            {
                queryParams = new Dictionary<string, string>();
            }

            Node current = root;
            Dictionary<string, object> routeArgs = new Dictionary<string, object>();

            foreach (string element in route.Split('/'))
            {
                if (string.IsNullOrEmpty (element)) continue;
                
                if (current.TryGetSubNode(element, out current!))
                {
                    continue;
                }
                if (ArgType.TryParse(element, out object? value, out Type? type)
                    && current.TryGetArgSubNode(type!, out current!))
                {
                    routeArgs[current.Name] = value!;
                }
                else
                {
                    return await Task.FromResult(new ResponseAction(false, null, null));
                }
            }


            if (current.Method != null)
            {
                if (!callers!.TryGetValue(current.Method.CallerType, out object? caller))
                {
                    return await Task.FromResult(new ResponseAction(false, null, null));
                }

                List<object> methodParams = new List<object>();

                if (current.Method.Function.GetParameters().Length > 0)
                {
                    foreach (ParameterInfo param in current.Method.Function.GetParameters())
                    {
                        if (param.ParameterType == typeof(Session))
                        {
                            methodParams.Add(session);
                            continue;
                        }
                        if (param.ParameterType == typeof(Dictionary<string, string>))
                        {
                            methodParams.Add(queryParams);
                            break;
                        }
                        if (routeArgs.TryGetValue(param.Name!, out object? routeValue))
                        {
                            methodParams.Add(routeValue);
                            continue;
                        }
                        if (queryParams.TryGetValue(param.Name!, out string? value))
                        {
                            if (param.ParameterType == typeof(string[]))
                            {
                                methodParams.Add(value.Split('&'));
                            }
                            else
                            {
                                methodParams.Add(value);
                            }
                        }
                        else
                        {
                            return await Task.FromResult(new ResponseAction(false, null, null));
                        }
                    }
                }

                object? ret = current.Method.Function.Invoke(caller, methodParams.ToArray());
                Type? retType = current.Method.Function.ReturnType;

                if (retType == typeof(Task))
                {
                    ret = null;
                    retType = null;
                }
                else if (retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    Task task = (Task)ret!;
                    await task!.ConfigureAwait(false);

                    ret = task.GetType().GetProperty(nameof(Task<object>.Result))!.GetValue(task);
                    retType = ret!.GetType();
                }
                return new ResponseAction(true, ret, retType);
            }
            return await Task.FromResult(new ResponseAction(false, null, null));
        }
    }
}
