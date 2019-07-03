-- Rotate line by 30 degrees counter-clockwise around (0 0)
select AffineTransform::Rotate(45).Apply('LINESTRING (5 0, 10 0)').ToString()
-- Returns: LINESTRING (3.5355339059327378 3.5355339059327373, 7.0710678118654755 7.0710678118654746)

-- Decrease line size 5 times
select AffineTransform::Scale(0.2, 0.2).Apply('LINESTRING (5 0, 10 0)').ToString()
-- Returns: LINESTRING (1 0, 2 0)

-- Move line down by 2 units
select AffineTransform::Translate(0, -2).Apply('LINESTRING (5 0, 10 0)').ToString()
-- Returns: LINESTRING (5 -2, 10 -2)