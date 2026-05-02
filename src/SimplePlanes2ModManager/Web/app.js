(function () {
  var state = null;
  var language = "zh-CN";

  var i18n = {
    "zh-CN": {
      appTitle: "SimplePlanes 2 插件管理器",
      brandTitle: "插件管理器",
      brandSubtitle: "SimplePlanes 2",
      overview: "概览",
      plugins: "插件",
      tools: "工具",
      status: "状态",
      loading: "加载中...",
      noGame: "未选择游戏目录。",
      refresh: "刷新",
      selectGameFolder: "选择游戏目录",
      game: "游戏",
      unknown: "未知",
      waitingGame: "等待选择游戏目录。",
      openGameFolder: "打开游戏目录",
      installedPlugins: "已安装插件",
      installBundledBepInEx: "安装内置 BepInEx",
      installBepInExZip: "安装 BepInEx 压缩包",
      openConfig: "打开配置目录",
      installPluginZip: "安装插件压缩包",
      openPluginsFolder: "打开插件目录",
      folders: "目录",
      installRules: "安装规则",
      ruleCloseGame: "修改插件前请先关闭游戏。",
      ruleZipSafe: "插件压缩包只会解压到已选择的游戏目录内。",
      ruleKeepConfig: "卸载插件时会保留 BepInEx 配置文件。",
      ruleDisabled: "禁用插件会把 DLL 重命名为 .dll.disabled。",
      gameReady: "游戏目录已就绪。",
      selectValidGame: "请选择有效的游戏目录。",
      valid: "有效",
      missing: "缺失",
      found: "已找到",
      incomplete: "不完整",
      installed: "已安装",
      noPlugins: "没有找到已安装插件。",
      enabled: "已启用",
      disabled: "已禁用",
      version: "版本",
      enable: "启用",
      disable: "禁用",
      uninstall: "卸载",
      done: "完成。",
      bridgeUnavailable: "桥接方法不可用：",
      operationFailed: "操作失败。",
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
      uninstall: "Uninstall",
      done: "Done.",
      bridgeUnavailable: "Bridge method is unavailable: ",
      operationFailed: "Operation failed.",
      switchLanguage: "中文"
    }
  };

  function dictionary() {
    return i18n[language] || i18n["zh-CN"];
  }

  function text(key) {
    var currentDictionary = dictionary();
    return currentDictionary[key] || key;
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
    if (state && state.settings && state.settings.language === "en-US") {
      language = "en-US";
    } else {
      language = "zh-CN";
    }

    updateUiLanguage();
    renderState();
  }

  function runAndRefresh(method, argument) {
    var response = callBridge(method, argument);
    if (!response || response.cancelled) {
      return;
    }

    if (!response.ok) {
      if (response.message) {
        showMessage(response.message, true);
      }
      return;
    }

    if (response.data && response.data.game) {
      state = response.data;
      if (state.settings && state.settings.language === "en-US") {
        language = "en-US";
      } else {
        language = "zh-CN";
      }
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
        '<button type="button" class="button secondary plugin-action" data-method="' + toggleMethod + '" data-plugin-id="' + escapeAttribute(plugin.id) + '">' + toggleText + '</button>' +
        '<button type="button" class="button danger plugin-action" data-method="UninstallPlugin" data-plugin-id="' + escapeAttribute(plugin.id) + '">' + text("uninstall") + '</button>' +
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
      text("version") + ': ' + escapeHtml(plugin.version || "--") +
      '</div>' +
      actions +
      '</div>'
    );
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
