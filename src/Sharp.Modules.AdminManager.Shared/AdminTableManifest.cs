namespace Sharp.Modules.AdminManager.Shared;

/*{
     "permissionSets": {
       "system": ["system:role:create", "system:role:delete"],
       "pluginA": ["pluginA:getWeapon", "pluginA:fetchItems"],
       "pluginB": ["pluginB:kick", "pluginB:slay"]
     },

     "roles": [
       { "name": "global_root", "permissions": ["*"] },
       { "name": "pluginA_admin", "permissions": ["pluginA:*"] },
       { "name": "pluginB_kicker", "permissions": ["pluginB:kick"] }
     ],

     "users": [
       {
         "user_id": "u100",
         // Permission array: @ prefix means inherit from role, others are direct permissions
         "permissions": ["@global_root", "!pluginB:slay"]
         // Resolved: all permissions from global_root (*) + directly deny pluginB:slay
       },
       {
         "user_id": "u104",
         "permissions": ["@pluginA_admin", "@pluginB_kicker"]
         // Resolved: pluginA:* (all of plugin A) + pluginB:kick (plugin B kick)
       },
       {
         "user_id": "u105",
         "permissions": ["pluginA:getWeapon", "!pluginA:fetchItems"]
         // Resolved: only directly allow getWeapon + directly deny fetchItems (no role inheritance)
       },
       {
         "user_id": "u106",
         "permissions": ["@pluginA_admin", "pluginB:slay"]
         // Resolved: pluginA:* (from role) + directly allow pluginB:slay
       }
     ]
   }*/

public record AdminTableManifest(
    Dictionary<string, HashSet<string>> PermissionCollection,
    List<RoleManifest> Roles,
    List<AdminManifest> Admins
);

public record RoleManifest(
    string Name,
    HashSet<string> Permissions
);

public record AdminManifest(
    string Name,
    ulong Identity,
    byte Immunity,
    HashSet<string> Permissions
);