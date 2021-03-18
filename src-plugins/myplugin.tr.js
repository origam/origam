function Root(props) {
  return /*#__PURE__*/React.createElement("div", null, "Hello, ", props.name, "!");
}

window.ORIGAM_PLUGINS.register("myplugin", Root);
