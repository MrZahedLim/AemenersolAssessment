-- Part 2: last updated well for each platform.

WITH RankedWells AS
(
    SELECT
        w.Id,
        w.PlatformId,
        w.UniqueName,
        w.Latitude,
        w.Longitude,
        w.CreatedAt,
        w.UpdatedAt,
        ROW_NUMBER() OVER (
            PARTITION BY w.PlatformId
            ORDER BY w.UpdatedAt DESC, w.Id DESC
        ) AS rn
    FROM Well AS w
)
SELECT
    p.UniqueName AS PlatformName,
    rw.Id,
    rw.PlatformId,
    rw.UniqueName,
    rw.Latitude,
    rw.Longitude,
    rw.CreatedAt,
    rw.UpdatedAt
FROM Platform AS p
INNER JOIN RankedWells AS rw
    ON rw.PlatformId = p.Id
   AND rw.rn = 1
ORDER BY p.Id;
