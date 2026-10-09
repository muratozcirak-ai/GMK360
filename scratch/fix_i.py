with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

content = content.replace('<div class="sub-blocks-list"></div>\n                                                </div>\n                                            </div>\n                                        </div>\n                                    }\n                                }', '<div class="sub-blocks-list"></div>\n                                                </div>\n                                            </div>\n                                        </div>\n                                        i++;\n                                    }\n                                }')

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Replaced i++")
