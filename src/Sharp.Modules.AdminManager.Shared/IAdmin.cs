/*
 * ModSharp
 * Copyright (C) 2023-2025 Kxnrl. All Rights Reserved.
 *
 * This file is part of ModSharp.
 * ModSharp is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * ModSharp is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with ModSharp. If not, see <https://www.gnu.org/licenses/>.
 */

using Sharp.Shared.Units;

namespace Sharp.Modules.AdminManager.Shared;

public interface IAdmin
{
    /// <summary>
    ///     Admin name
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     SteamID
    /// </summary>
    SteamID Identity { get; }

    /// <summary>
    ///     Immunity level
    /// </summary>
    byte Immunity { get; }

    /// <summary>
    ///     Permissions
    /// </summary>
    IReadOnlySet<string> Permissions { get; }

    /// <summary>
    ///     Check if has permission
    /// </summary>
    /// <param name="permission">Permission field</param>
    /// <returns></returns>
    bool HasPermission(string permission);

    /// <summary>
    ///     Add permission
    /// </summary>
    /// <param name="permission">Permission field</param>
    /// <returns></returns>
    bool AddPermission(string permission);

    /// <summary>
    ///     Remove permission
    /// </summary>
    /// <param name="permission">Permission field</param>
    /// <returns></returns>
    bool RemovePermission(string permission);
}