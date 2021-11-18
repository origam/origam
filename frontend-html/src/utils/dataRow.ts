export function fixRowIdentifier(row: any[], identifierIndex: number) {
    row = [...row];
    const id = row[identifierIndex];
    if(id !== null && id !==undefined) {
        row[identifierIndex] = `${id}`;
    }
    return row;
}