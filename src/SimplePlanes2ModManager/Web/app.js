(function () {
  var state = null;

  function callBridge(method, argument) {
    try {
      if (!window.external || !window.external[method]) {
        showMessage("Bridge method is unavailable: " + method, true);
        return null;
      }

      var raw = argument === undefined ? window.external[method]() : window.external[method](argument);
      var response = JSON.parse(raw);
      if (!response.ok && !response.cancelled) {
        showMessage(response.message || "Operation failed.", true);
      }

      return response;
    } catch (error) {
      showMessage(String(error), true);
      return null;
    }
  }

  function refresh() {
    var response = callBridge("GetState");
    if (!response || !response.ok) {
      return;
    }

    state = response.data;
    renderState();
  }

  function runAndRefresh(method, argument) {
    var response = callBridge(method, argument);
    if (!response || response.cancelled) {
      return;
    }

    if (response.ok && response.data && response.data.game) {
      state = response.data;
      renderState();
      showMessage("Done.", false);
      return;
    }

    refresh();
  }

  function renderState() {
    if (!state) {
      return;
    }

    renderGameState();
    renderBepInExState();
    renderPlugins();
  }

  function renderGameState() {
    var game = state.game || {};
    setText("gamePathText", game.path || "No game folder selected.");
    setText("gameMessage", game.message || "");
    setText("sidebarStatus", game.isValid ? "Game folder ready." : "Select a valid game folder.");
    setBadge("gameBadge", game.isValid ? "Valid" : "Missing", game.isValid ? "good" : "bad");

    setDisabled("openGameButton", !game.isValid);
    setDisabled("toolsOpenGameButton", !game.isValid);
  }

  function renderBepInExState() {
    var bepinex = state.bepinex || {};
    setBadge("bepinexBadge", bepinex.isInstalled ? "Installed" : "Incomplete", bepinex.isInstalled ? "good" : "warn");
    setText("doorstopState", bepinex.hasDoorstop ? "Found" : "Missing");
    setText("proxyState", bepinex.hasProxyDll ? "Found" : "Missing");
    setText("coreState", bepinex.hasCoreDll ? "Found" : "Missing");
  }

  function renderPlugins() {
    var plugins = state.plugins || [];
    setText("pluginCountBadge", String(plugins.length));
    renderPluginContainer("overviewPlugins", plugins, false);
    renderPluginContainer("pluginList", plugins, true);
  }

  function renderPluginContainer(elementId, plugins, includeActions) {
    var container = document.getElementById(elementId);
    if (!container) {
      return;
    }

    if (!plugins.length) {
      container.innerHTML = '<div class="muted">No installed plugins found.</div>';
      return;
    }

    var html = "";
    for (var index = 0; index < plugins.length; index++) {
      html += renderPluginCard(plugins[index], includeActions);
    }

    container.innerHTML = html;
  }

  function renderPluginCard(plugin, includeActions) {
    var statusClass = plugin.isEnabled ? "good" : "warn";
    var statusText = plugin.isEnabled ? "Enabled" : "Disabled";
    var actions = "";

    if (includeActions) {
      var toggleMethod = plugin.isEnabled ? "DisablePlugin" : "EnablePlugin";
      var toggleText = plugin.isEnabled ? "Disable" : "Enable";
      actions =
        '<div class="plugin-actions">' +
        '<button class="button secondary" onclick="Manager.invokePluginAction(\\'' + toggleMethod + '\\', \\'' + escapeAttribute(plugin.id) + '\\')">' + toggleText + '</button>' +
        '<button class="button danger" onclick="Manager.invokePluginAction(\\'UninstallPlugin\\', \\'' + escapeAttribute(plugin.id) + '\\')">Uninstall</button>' +
        '</div>';
    }

    return (
      '<div class="plugin-card">' +
      '<div class="plugin-title">' +
      '<span>' + escapeHtml(plugin.name || plugin.id) + '</span>' +
      '<span class="badge ' + statusClass + '">' + statusText + '</span>' +
      '</div>' +
      '<div class="plugin-meta">' +
      escapeHtml(plugin.entryFile || "--") + '<br>' +
      'Version: ' + escapeHtml(plugin.version || "--") +
      '</div>' +
      actions +
      '</div>'
    );
  }

  function showMessage(message, isError) {
    var box = document.getElementById("messageBox");
    if (!box) {
      return;
    }

    box.className = isError ? "message error" : "message";
    box.innerText = message;
    if (!isError) {
      window.setTimeout(function () {
        box.className = "message hidden";
      }, 1800);
    }
  }

  function setText(id, text) {
    var element = document.getElementById(id);
    if (element) {
      element.innerText = text;
    }
  }

  function setBadge(id, text, className) {
    var element = document.getElementById(id);
    if (element) {
      element.className = "badge " + className;
      element.innerText = text;
    }
  }

  function setDisabled(id, disabled) {
    var element = document.getElementById(id);
    if (element) {
      element.disabled = !!disabled;
    }
  }

  function escapeHtml(value) {
    return String(value)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function escapeAttribute(value) {
    return escapeHtml(value).replace(/'/g, "&#39;");
  }

  function switchSection(sectionId) {
    var sections = document.querySelectorAll(".section");
    var navItems = document.querySelectorAll(".nav-item");
    for (var index = 0; index < sections.length; index++) {
      sections[index].className = sections[index].id === sectionId ? "section active" : "section";
    }

    for (var navIndex = 0; navIndex < navItems.length; navIndex++) {
      var item = navItems[navIndex];
      item.className = item.getAttribute("data-section") === sectionId ? "nav-item active" : "nav-item";
    }
  }

  function bindClick(id, callback) {
    var element = document.getElementById(id);
    if (element) {
      element.onclick = callback;
    }
  }

  function bindEvents() {
    var navItems = document.querySelectorAll(".nav-item");
    for (var index = 0; index < navItems.length; index++) {
      navItems[index].onclick = function () {
        switchSection(this.getAttribute("data-section"));
      };
    }

    bindClick("refreshButton", refresh);
    bindClick("browseButton", function () { runAndRefresh("BrowseGameDirectory"); });
    bindClick("installPluginButton", function () { runAndRefresh("SelectAndInstallPluginZip"); });
    bindClick("installBundledBepInExButton", function () { runAndRefresh("InstallBundledBepInEx"); });
    bindClick("installBepInExButton", function () { runAndRefresh("SelectAndInstallBepInExZip"); });
    bindClick("openGameButton", function () { runAndRefresh("OpenGameDirectory"); });
    bindClick("openPluginsButton", function () { runAndRefresh("OpenPluginsDirectory"); });
    bindClick("openConfigButton", function () { runAndRefresh("OpenConfigDirectory"); });
    bindClick("toolsOpenGameButton", function () { runAndRefresh("OpenGameDirectory"); });
    bindClick("toolsOpenPluginsButton", function () { runAndRefresh("OpenPluginsDirectory"); });
    bindClick("toolsOpenConfigButton", function () { runAndRefresh("OpenConfigDirectory"); });
  }

  window.Manager = {
    invokePluginAction: function (method, pluginId) {
      runAndRefresh(method, pluginId);
    }
  };

  bindEvents();
  refresh();
})();
