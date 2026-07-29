```CSharp
  

Enum standard_type {

  zalo

  facebook

  instagram

}

  

Enum post_output_type {

  Flower

  Post

  Card

}

  

Enum output_type {

  Flower

  Post

  Card

}

  

Enum system_prompts_type {

  Flower

  Post

  Card

}

  

Table system_prompts {

  id uuid [pk]

  type system_prompts_type

  content text

}

  

Table standards {

  id uuid [pk]

  description text

  type standard_type

}

  

Table products {

  base_id uuid

  id uuid [pk]

}

  

Table template_cards_base {

  base_id uuid

  name text

  description text// mo ta y nghia cua template

  limit_text int  

  picture_url text

  metadata json//chua size,... cua template

  id uuid [pk]

}

  

Table post_cores_base {

  id uuid [pk]

  base_id uuid

  content text

  picture_url timestamp

}

  

Table histories {

  id uuid [pk]

  input text

  base_id uuid

  output_id uuid

  type output_type

  system_prompt_id uuid

  metadata json

}

  

Table post_outs {

  id uuid [pk]

  output_id uuid [unique]

  content text

  image_url varchar

  user_id uuid

  order_id uuid

}

  

Table flower_outs {

  id uuid [pk]

  // card_out uuid

  output_id uuid [unique]

  image_url varchar

  user_id uuid

  order_id uuid

}

  

Table card_outs {

  id uuid [pk]

  output_id uuid [unique]

  content text

  image_url varchar

  user_id uuid

  order_id uuid

}

  
  

// Ref: flower_outs.card_out < card_outs.id

Ref: histories.system_prompt_id > system_prompts.id

Ref: histories.base_id > products.base_id

Ref: histories.base_id > post_cores_base.base_id

Ref: histories.base_id > template_cards_base.base_id

Ref: post_outs.output_id - histories.output_id

Ref: flower_outs.output_id - histories.output_id

Ref: card_outs.output_id - histories.output_id
```