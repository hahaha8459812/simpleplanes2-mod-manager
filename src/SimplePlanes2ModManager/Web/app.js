(function () {
  var state = null;
  var language = "zh-CN";

  var i18n = {
    "zh-CN": {
      appTitle: "\u0053\u0069\u006d\u0070\u006c\u0065\u0050\u006c\u0061\u006e\u0065\u0073\u0020\u0032\u0020\u63d2\u4ef6\u7ba1\u7406\u5668",
      brandTitle: "\u63d2\u4ef6\u7ba1\u7406\u5668",
      brandSubtitle: "SimplePlanes 2",
      overview: "\u6982\u89c8",
      plugins: "\u63d2\u4ef6",
      tools: "\u5de5\u5177",
      status: "\u72b6\u6001",
      loading: "\u52a0\u8f7d\u4e2d...",
      noGame: "\u672a\u9009\u62e9\u6e38\u620f\u76ee\u5f55\u3002",
      refresh: "\u5237\u65b0",
      selectGameFolder: "\u9009\u62e9\u6e38\u620f\u76ee\u5f55",
      game: "\u6e38\u620f",
      unknown: "\u672a\u77e5",
      waitingGame: "\u7b49\u5f85\u9009\u62e9\u6e38\u620f\u76ee\u5f55\u3002",
      openGameFolder: "\u6253\u5f00\u6e38\u620f\u76ee\u5f55",
      installedPlugins: "\u5df2\u5b89\u88c5\u63d2\u4ef6",
      installBundledBepInEx: "\u5b89\u88c5\u5185\u7f6e BepInEx",
      installBepInExZip: "\u5b89\u88c5 BepInEx \u538b\u7f29\u5305",
      openConfig: "\u6253\u5f00\u914d\u7f6e\u76ee\u5f55",
      installPluginZip: "\u5b89\u88c5\u63d2\u4ef6\u538b\u7f29\u5305",
      installFromGit: "\u4ece Git \u5b89\u88c5",
      checkUpdates: "\u68c0\u67e5\u66f4\u65b0",
      gitUrlPlaceholder: "GitHub \u4ed3\u5e93\u5730\u5740\u6216 index.json \u5730\u5740",
      openPluginsFolder: "\u6253\u5f00\u63d2\u4ef6\u76ee\u5f55",
      folders: "\u76ee\u5f55",
      installRules: "\u5b89\u88c5\u89c4\u5219",
      ruleCloseGame: "\u4fee\u6539\u63d2\u4ef6\u524d\u8bf7\u5148\u5173\u95ed\u6e38\u620f\u3002",
      ruleZipSafe: "\u63d2\u4ef6\u538b\u7f29\u5305\u53ea\u4f1a\u89e3\u538b\u5230\u5df2\u9009\u62e9\u7684\u6e38\u620f\u76ee\u5f55\u5185\u3002",
      ruleKeepConfig: "\u5378\u8f7d\u63d2\u4ef6\u65f6\u4f1a\u4fdd\u7559 BepInEx \u914d\u7f6e\u6587\u4ef6\u3002",
      ruleDisabled: "\u7981\u7528\u63d2\u4ef6\u4f1a\u628a DLL \u91cd\u547d\u540d\u4e3a .dll.disabled\u3002",
      gameReady: "\u6e38\u620f\u76ee\u5f55\u5df2\u5c31\u7eea\u3002",
      selectValidGame: "\u8bf7\u9009\u62e9\u6709\u6548\u7684\u6e38\u620f\u76ee\u5f55\u3002",
      valid: "\u6709\u6548",
      missing: "\u7f3a\u5931",
      found: "\u5df2\u627e\u5230",
      incomplete: "\u4e0d\u5b8c\u6574",
      installed: "\u5df2\u5b89\u88c5",
      noPlugins: "\u6ca1\u6709\u627e\u5230\u5df2\u5b89\u88c5\u63d2\u4ef6\u3002",
      enabled: "\u5df2\u542f\u7528",
      disabled: "\u5df2\u7981\u7528",
      version: "\u7248\u672c",
      enable: "\u542f\u7528",
      disable: "\u7981\u7528",
      update: "\u66f4\u65b0",
      latest: "\u6700\u65b0",
      source: "\u6765\u6e90",
      updateAvailable: "\u6709\u65b0\u7248\u672c",
      updateCheckFailed: "\u68c0\u67e5\u5931\u8d25",
      noUpdateSource: "\u65e0\u66f4\u65b0\u6765\u6e90",
      uninstall: "\u5378\u8f7d",
      done: "\u5b8c\u6210\u3002",
      bridgeUnavailable: "\u6865\u63a5\u65b9\u6cd5\u4e0d\u53ef\u7528\uff1a",
      operationFailed: "\u64cd\u4f5c\u5931\u8d25\u3002",
      switchLanguage: "English"
    },
    "en-US": {
      appTitle: "SimplePlanes 2 Mod Manager",
      brandTitle: "Mod Manager",
      brandSubtitle: "SimplePlanes 2",
      overview: "Overview",
      plugins: "Plugins",
      tools: "Tools",
      status: "Status",
      loading: "Loading...",
      noGame: "No game folder selected.",
      refresh: "Refresh",
      selectGameFolder: "Select Game Folder",
      game: "Game",
      unknown: "Unknown",
      waitingGame: "Waiting for game directory.",
      openGameFolder: "Open Game Folder",
      installedPlugins: "Installed Plugins",
      installBundledBepInEx: "Install Bundled BepInEx",
      installBepInExZip: "Install BepInEx Zip",
      openConfig: "Open Config",
      installPluginZip: "Install Plugin Zip",
      installFromGit: "Install from Git",
      checkUpdates: "Check Updates",
      gitUrlPlaceholder: "GitHub repository URL or index.json URL",
      openPluginsFolder: "Open Plugins Folder",
      folders: "Folders",
      installRules: "Install Rules",
      ruleCloseGame: "Close the game before changing plugins.",
      ruleZipSafe: "Plugin zips are extracted only inside the selected game folder.",
      ruleKeepConfig: "Uninstall keeps BepInEx config files.",
      ruleDisabled: "Disabled plugins are renamed to .dll.disabled.",
      gameReady: "Game folder ready.",
      selectValidGame: "Select a valid game folder.",
      valid: "Valid",
      missing: "Missing",
      found: "Found",
      incomplete: "Incomplete",
      installed: "Installed",
      noPlugins: "No installed plugins found.",
      enabled: "Enabled",
      disabled: "Disabled",
      version: "Version",
      enable: "Enable",
      disable: "Disable",
      update: "Update",
      latest: "Latest",
      source: "Source",
      updateAvailable: "Update Available",
      updateCheckFailed: "Check Failed",
      noUpdateSource: "No Update Source",
      uninstall: "Uninstall",
      done: "Done.",
      bridgeUnavailable: "Bridge method is unavailable: ",
      operationFailed: "Operation failed.",
      switchLanguage: "\u4e2d\u6587"
    }
  };

  function text(key) {
    var dictionary = i18n[language] || i18n["zh-CN"];
    return dictionary[key] || key;
  }

  function updateUiLanguage() {
    document.title = text("appTitle");
    setText("brandTitle", text("brandTitle"));
    setText("brandSubtitle", text("brandSubtitle"));
    setText("navOverview", text("overview"));
    setText("navPlugins", text("plugins"));
    setText("navTools", text("tools"));
    setText("sidebarStatusLabel", text("status"));
    setText("mainTitle", text("appTitle"));
    setText("refreshButton", text("refresh"));
    setText("browseButton", text("selectGameFolder"));
    setText("gameTitle", text("game"));
    setText("openGameButton", text("openGameFolder"));
    setText("installedPluginsTitle", text("installedPlugins"));
    setText("installBundledBepInExButton", text("installBundledBepInEx"));
    setText("installBepInExButton", text("installBepInExZip"));
    setText("openConfigButton", text("openConfig"));
    setText("pluginsTitle", text("plugins"));
    setText("installFromGitButton", text("installFromGit"));
    setText("checkUpdatesButton", text("checkUpdates"));
    setInputPlaceholder("gitInstallUrlInput", text("gitUrlPlaceholder"));
    setText("installPluginButton", text("installPluginZip"));
    setText("openPluginsButton", text("openPluginsFolder"));
    setText("foldersTitle", text("folders"));
    setText("toolsOpenGameButton", text("openGameFolder"));
    setText("toolsOpenPluginsButton", text("openPluginsFolder"));
    setText("toolsOpenConfigButton", text("openConfig"));
    setText("installRulesTitle", text("installRules"));
    setText("ruleCloseGame", text("ruleCloseGame"));
    setText("ruleZipSafe", text("ruleZipSafe"));
    setText("ruleKeepConfig", text("ruleKeepConfig"));
    setText("ruleDisabled", text("ruleDisabled"));
    setText("languageButton", text("switchLanguage"));
  }

  function callBridge(method, argument) {
    try {
      if (!window.external) {
        showMessage(text("bridgeUnavailable") + method, true);
        return null;
      }

      var raw = null;
      if (method === "GetState") {
        raw = window.external.GetState();
      } else if (method === "BrowseGameDirectory") {
        raw = window.external.BrowseGameDirectory();
      } else if (method === "SetLanguage") {
        raw = window.external.SetLanguage(argument);
      } else if (method === "SelectAndInstallPluginZip") {
        raw = window.external.SelectAndInstallPluginZip();
      } else if (method === "InstallPluginFromGit") {
        raw = window.external.InstallPluginFromGit(argument);
      } else if (method === "CheckPluginUpdates") {
        raw = window.external.CheckPluginUpdates();
      } else if (method === "UpdatePlugin") {
        raw = window.external.UpdatePlugin(argument);
      } else if (method === "InstallBundledBepInEx") {
        raw = window.external.InstallBundledBepInEx();
      } else if (method === "SelectAndInstallBepInExZip") {
        raw = window.external.SelectAndInstallBepInExZip();
      } else if (method === "OpenGameDirectory") {
        raw = window.external.OpenGameDirectory();
      } else if (method === "OpenPluginsDirectory") {
        raw = window.external.OpenPluginsDirectory();
      } else if (method === "OpenConfigDirectory") {
        raw = window.external.OpenConfigDirectory();
      } else if (method === "EnablePlugin") {
        raw = window.external.EnablePlugin(argument);
      } else if (method === "DisablePlugin") {
        raw = window.external.DisablePlugin(argument);
      } else if (method === "UninstallPlugin") {
        raw = window.external.UninstallPlugin(argument);
      } else {
        showMessage(text("bridgeUnavailable") + method, true);
        return null;
      }

      return JSON.parse(raw);
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
    language = state && state.settings && state.settings.language === "en-US" ? "en-US" : "zh-CN";
    updateUiLanguage();
    renderState();
  }

  function runAndRefresh(method, argument) {
    var response = callBridge(method, argument);
    if (!response || response.cancelled) {
      return;
    }

    if (!response.ok) {
      showMessage(response.message || text("operationFailed"), true);
      return;
    }

    if (response.data && response.data.game) {
      state = response.data;
      language = state.settings && state.settings.language === "en-US" ? "en-US" : "zh-CN";
      updateUiLanguage();
      renderState();
      showMessage(text("done"), false);
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
    setText("gamePathText", game.path || text("noGame"));
    setText("gameMessage", game.message || text("waitingGame"));
    setText("sidebarStatus", game.isValid ? text("gameReady") : text("selectValidGame"));
    setBadge("gameBadge", game.isValid ? text("valid") : text("missing"), game.isValid ? "good" : "bad");
    setDisabled("openGameButton", !game.isValid);
    setDisabled("toolsOpenGameButton", !game.isValid);
  }

  function renderBepInExState() {
    var bepinex = state.bepinex || {};
    setBadge("bepinexBadge", bepinex.isInstalled ? text("installed") : text("incomplete"), bepinex.isInstalled ? "good" : "warn");
    setText("doorstopState", bepinex.hasDoorstop ? text("found") : text("missing"));
    setText("proxyState", bepinex.hasProxyDll ? text("found") : text("missing"));
    setText("coreState", bepinex.hasCoreDll ? text("found") : text("missing"));
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
      container.innerHTML = '<div class="muted">' + escapeHtml(text("noPlugins")) + '</div>';
      return;
    }

    var html = "";
    for (var index = 0; index < plugins.length; index++) {
      html += renderPluginCard(plugins[index], includeActions);
    }

    container.innerHTML = html;
    bindPluginActions(container);
  }

  function renderPluginCard(plugin, includeActions) {
    var statusClass = plugin.isEnabled ? "good" : "warn";
    var statusText = plugin.isEnabled ? text("enabled") : text("disabled");
    var actions = "";

    if (includeActions) {
      var toggleMethod = plugin.isEnabled ? "DisablePlugin" : "EnablePlugin";
      var toggleText = plugin.isEnabled ? text("disable") : text("enable");
      actions =
        '<div class="plugin-actions">' +
        renderUpdateButton(plugin) +
        '<button type="button" class="button secondary plugin-action" data-method="' + toggleMethod + '" data-plugin-id="' + escapeAttribute(plugin.id) + '">' + toggleText + '</button>' +
        '<button type="button" class="button danger plugin-action" data-method="UninstallPlugin" data-plugin-id="' + escapeAttribute(plugin.id) + '">' + text("uninstall") + '</button>' +
        '</div>';
    }

    var updateBadge = renderUpdateBadge(plugin);
    var updateDetails = renderUpdateDetails(plugin);
    return (
      '<div class="plugin-card">' +
      '<div class="plugin-title">' +
      '<span>' + escapeHtml(plugin.name || plugin.id) + '</span>' +
      '<span>' + updateBadge + '<span class="badge ' + statusClass + '">' + statusText + '</span></span>' +
      '</div>' +
      '<div class="plugin-meta">' +
      escapeHtml(plugin.entryFile || "--") + '<br>' +
      text("version") + ': ' + escapeHtml(plugin.installedVersion || plugin.version || "--") +
      updateDetails +
      '</div>' +
      actions +
      '</div>'
    );
  }

  function renderUpdateButton(plugin) {
    if (!plugin.updateAvailable) {
      return "";
    }

    return '<button type="button" class="button primary plugin-action" data-method="UpdatePlugin" data-plugin-id="' + escapeAttribute(plugin.id) + '">' + text("update") + '</button>';
  }

  function renderUpdateBadge(plugin) {
    if (plugin.updateAvailable) {
      return '<span class="badge warn">' + text("updateAvailable") + '</span> ';
    }

    if (plugin.updateCheckFailed) {
      return '<span class="badge bad">' + text("updateCheckFailed") + '</span> ';
    }

    return "";
  }

  function renderUpdateDetails(plugin) {
    var html = "";
    if (plugin.latestVersion) {
      html += '<br>' + text("latest") + ': ' + escapeHtml(plugin.latestVersion);
    }

    if (plugin.repository || plugin.indexUrl) {
      html += '<br>' + text("source") + ': ' + escapeHtml(plugin.repository || plugin.indexUrl);
    } else if (!plugin.canCheckUpdates) {
      html += '<br>' + text("source") + ': ' + text("noUpdateSource");
    }

    if (plugin.updateMessage) {
      html += '<br>' + escapeHtml(plugin.updateMessage);
    }

    return html;
  }

  function bindPluginActions(container) {
    var buttons = container.getElementsByTagName("button");
    for (var index = 0; index < buttons.length; index++) {
      if ((" " + buttons[index].className + " ").indexOf(" plugin-action ") < 0) {
        continue;
      }

      buttons[index].onclick = function () {
        runAndRefresh(this.getAttribute("data-method"), this.getAttribute("data-plugin-id"));
      };
    }
  }

  function showMessage(message, isError) {
    var box = document.getElementById("messageBox");
    if (!box) {
      return;
    }

    box.className = isError ? "message error" : "message";
    setElementText(box, message);
    if (!isError) {
      window.setTimeout(function () {
        box.className = "message hidden";
      }, 1800);
    }
  }

  function setText(id, value) {
    var element = document.getElementById(id);
    if (element) {
      setElementText(element, value);
    }
  }

  function setElementText(element, value) {
    if ("textContent" in element) {
      element.textContent = value;
    } else {
      element.innerText = value;
    }
  }

  function setBadge(id, value, className) {
    var element = document.getElementById(id);
    if (element) {
      element.className = "badge " + className;
      setElementText(element, value);
    }
  }

  function setDisabled(id, disabled) {
    var element = document.getElementById(id);
    if (element) {
      element.disabled = !!disabled;
    }
  }

  function setInputPlaceholder(id, value) {
    var element = document.getElementById(id);
    if (element) {
      element.setAttribute("placeholder", value);
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

  function getElementsByClassName(className) {
    if (document.getElementsByClassName) {
      return document.getElementsByClassName(className);
    }

    var allElements = document.getElementsByTagName("*");
    var matchedElements = [];
    for (var index = 0; index < allElements.length; index++) {
      if ((" " + allElements[index].className + " ").indexOf(" " + className + " ") >= 0) {
        matchedElements.push(allElements[index]);
      }
    }

    return matchedElements;
  }

  function switchSection(sectionId) {
    var sections = getElementsByClassName("section");
    var navItems = getElementsByClassName("nav-item");
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
    var navItems = getElementsByClassName("nav-item");
    for (var index = 0; index < navItems.length; index++) {
      navItems[index].onclick = function () {
        switchSection(this.getAttribute("data-section"));
      };
    }

    bindClick("languageButton", function () {
      runAndRefresh("SetLanguage", language === "zh-CN" ? "en-US" : "zh-CN");
    });
    bindClick("refreshButton", refresh);
    bindClick("browseButton", function () { runAndRefresh("BrowseGameDirectory"); });
    bindClick("installFromGitButton", function () {
      var input = document.getElementById("gitInstallUrlInput");
      runAndRefresh("InstallPluginFromGit", input ? input.value : "");
    });
    bindClick("checkUpdatesButton", function () { runAndRefresh("CheckPluginUpdates"); });
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

  bindEvents();
  refresh();
})();
