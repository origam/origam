export function findFirstDFS(startNode: any, predicate: (cn: any) => boolean) {
  function dfs(cn: any) {
    if (predicate(cn)) {
      throw { $node: cn };
    }

    cn.elements && cn.elements.forEach((ch: any) => dfs(ch));
  }

  try {
    dfs(startNode);
  } catch (e) {
    if (e.$node) {
      return e.$node;
    }
    throw e;
  }
}

export function findFirstBFS(startNode: any, predicate: (cn: any) => boolean) {
  function bfs() {
    while (queue.length > 0) {
      const cn = queue.shift();
      if (predicate(cn)) {
        throw { $node: cn };
      }
      cn.elements && queue.push(...cn.elements);
    }
  }
  const queue: any[] = [startNode];
  try {
    bfs();
  } catch (e) {
    if (e.$node) {
      return e.$node;
    }
    throw e;
  }
}

export function findAll(startNode: any, predicate: (cn: any) => boolean) {
  function bfs() {
    while (queue.length > 0) {
      const cn = queue.shift();
      nodesWalked++;
      if (predicate(cn)) {
        result.push(cn);
      }
      cn.elements && queue.push(...cn.elements);
    }
  }
  const queue: any[] = [startNode];
  const result: any[] = [];
  let nodesWalked = 0;
  bfs();
  // console.log("NodesWalked:", nodesWalked);
  return result;
}

export function findAllStopping(
  startNode: any,
  predicate: (cn: any) => boolean
) {
  function bfs() {
    while (queue.length > 0) {
      const cn = queue.shift();
      nodesWalked++;
      if (predicate(cn)) {
        result.push(cn);
      } else {
        cn.elements && queue.push(...cn.elements);
      }
    }
  }
  const queue: any[] = [startNode];
  const result: any[] = [];
  let nodesWalked = 0;
  bfs();
  // console.log("NodesWalked:", nodesWalked);
  return result;
}
