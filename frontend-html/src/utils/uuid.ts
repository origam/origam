// https://stackoverflow.com/questions/105034/how-to-create-a-guid-uuid
// Math.random() is not the best rabdom number generator, should be ok for genrating Favorite
// folder ids. For a more robust solution, consider using the uuid module.
export function uuidv4() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}