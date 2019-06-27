-- Project point and linestring using Albers Equal Area projection
declare @albers Projection
set @albers = Projection::AlbersEqualArea(0, 0, 0, 60)

select @albers.Project('POINT (45 30)').ToString()
select @albers.Unproject(@albers.Project('LINESTRING (10 0, 10 10)')).ToString()
select @albers.ToString()